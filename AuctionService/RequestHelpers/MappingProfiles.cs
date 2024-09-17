using AutoMapper;
using CarsAppBackend.DTOs;
using CarsAppBackend.Entities;

namespace CarsAppBackend.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionDto>();
            CreateMap<CreateAuctionDto, Auction>()
                .ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<CreateAuctionDto, Item>();
            CreateMap<UpdateAuctionDto, Auction>()
                .ForAllMembers(opts => opts.Condition((src, dst, member) =>  member != null )); //ForAllMember Checks Each Property and mapping property that not equal to null from updateauction to auction

            CreateMap<UpdateAuctionDto, Item>()
                .ForAllMembers(opts => opts.Condition((src, dst, member) => member != null));
        }
    }
}
