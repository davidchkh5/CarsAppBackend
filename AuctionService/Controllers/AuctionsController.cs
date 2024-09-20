using AutoMapper;
using CarsAppBackend.Data;
using CarsAppBackend.DTOs;
using CarsAppBackend.Entities;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarsAppBackend.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }


        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctions = await _context.Auctions.Include(a => a.Item).OrderBy(a => a.Item.Make).ToListAsync();

            return _mapper.Map<List<AuctionDto>>(auctions);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a=> a.Id == id);

            if (auction == null) return NotFound("Auction not found");

            return Ok(_mapper.Map<AuctionDto>(auction));
        }


        [HttpPost]
        public async Task<ActionResult<CreateAuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            auction.Seller = "Test";
           await _context.Auctions.AddAsync(auction);

            var newAuction = _mapper.Map<AuctionDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            var result = await _context.SaveChangesAsync() > 0;


            if (!result) return BadRequest("Could not save changes to the DB");
             

            return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, newAuction);

        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(a=> a.Item).SingleOrDefaultAsync(a => a.Id == id);


            if (auction == null) return NotFound();


            //Check seller == username

            _mapper.Map(updateAuctionDto, auction);
            _mapper.Map(updateAuctionDto, auction.Item);

            await _publishEndpoint.Publish<AuctionCreated>(_mapper.Map<AuctionUpdated>(auction)); 

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem saving changes");    

        }


        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            _context.Auctions.Remove(auction);

            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _context.SaveChangesAsync() > 0;

            if(result) return Ok();

            return BadRequest("Problem deleting the item");
        }

    }
}
