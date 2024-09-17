
using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

namespace SearchService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
            builder.Services.AddMassTransit(x =>
            {

                x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

                x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
                
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


            //it helps to avoid looping of crashing application even if http throws error, this current service not to be crashed and be able to get requests
            app.Lifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    await DbInitializer.InitDb(app);
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                }
            });

 

            await app.RunAsync();

            //this thing catches the http error for service (for example if service 2 that we send to request is not avaiable ) and tries every 3 seconds until be avaiable
            static IAsyncPolicy<HttpResponseMessage> GetPolicy()
                => HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
            
            
        }
    }
}
