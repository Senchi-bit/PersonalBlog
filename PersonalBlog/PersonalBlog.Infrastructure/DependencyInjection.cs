using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalBlog.Domain.Articles;
using PersonalBlog.Infrastructure.Persistence.Mongo;
using PersonalBlog.Infrastructure.Time;
using SharedKernel;

namespace PersonalBlog.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services
                .AddOptions<PersonalBlogDatabaseSettings>()
                .Bind(configuration.GetSection(PersonalBlogDatabaseSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<PersonalBlogDatabaseSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            services.AddSingleton(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<PersonalBlogDatabaseSettings>>().Value;
                var client = serviceProvider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IArticleRepository, ArticleRepository>();
            services.AddHostedService<ArticleIndexInitializer>();

            return services;
        }
    }
}
