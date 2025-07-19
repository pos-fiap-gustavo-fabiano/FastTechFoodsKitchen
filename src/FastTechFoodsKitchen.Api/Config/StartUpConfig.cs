using FastTechFoods.Observability;
using MongoDB.Driver;

namespace FastTechFoodsKitchen.Api.Config
{
    public static class StartUpConfig
    {
        public static void AddObservability(WebApplicationBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE")
                ?? "mongodb://localhost:27017";

            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                return new MongoClient(connectionString);
            });
            builder.Services.AddFastTechFoodsObservabilityWithSerilog(builder.Configuration);
            builder.Services.AddFastTechFoodsPrometheus(builder.Configuration);
            //builder.Services.Configure<OpenTelemetry.Trace.TracerProviderBuilder>(tracerBuilder =>
            //{
            //    tracerBuilder
            //        .AddSource("FastTechFoodsKitchen.MessagePublisher")
            //        .AddSource("FastTechFoodsKitchen.KitchenOrderService")
            //        .AddSource("FastTechFoodsKitchen.Api")
            //        .AddSource("FastTechFoodsKitchen.Controllers")
            //        .AddSource("FastTechFoodsKitchen.Services");
            //});

            builder.Services.AddFastTechFoodsHealthChecksWithMongoDB(builder.Configuration, connectionString);
        }

        public static void UseObservability(WebApplication app)
        {
            app.UseFastTechFoodsHealthChecksUI();
            app.UseFastTechFoodsPrometheus();
        }
    }
}
