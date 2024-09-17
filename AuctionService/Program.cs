
using CarsAppBackend.Data;
using CarsAppBackend.Entities;
using Microsoft.EntityFrameworkCore;
using MassTransit;


namespace CarsAppBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<AuctionDbContext>(opt =>
            {
                opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            try
            {
                DbInitializer.InitDb(app);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

            app.Run();
        }
    }
}








