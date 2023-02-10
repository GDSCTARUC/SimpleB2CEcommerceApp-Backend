using ProductServer.Infrastructure.Context;
using ProductServer.Infrastructure.Models;

namespace ProductServer
{
    public class ProductWorker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ProductWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var products = new List<Product>();
            for (int i = 0; i < 100; i++)
            {
                var random = new Random();
                products.Add(new Product
                {
                    Name = $"Product {i}",
                    Description = $"Product {i} description",
                    OriginalPrice = random.Next(1, 1000),
                    DiscountPercentage = random.Next(1, 100),
                    ImageFileName = "apple_05a2a1fb-4159-49c0-ae49-c5d2caff3c53.jpg"
                });
            }

            await context.Products.AddRangeAsync(products, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
