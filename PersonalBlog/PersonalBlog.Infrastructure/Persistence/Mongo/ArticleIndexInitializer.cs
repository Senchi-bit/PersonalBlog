using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalBlog.Infrastructure.Persistence.Mongo.Documents;

namespace PersonalBlog.Infrastructure.Persistence.Mongo;

internal sealed class ArticleIndexInitializer(IMongoDatabase database,
    IOptions<PersonalBlogDatabaseSettings> settings,
    ILogger<ArticleIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var collection = database.GetCollection<ArticleDocument>(settings.Value.ArticlesCollectionName);
        var index = new CreateIndexModel<ArticleDocument>(
            Builders<ArticleDocument>.IndexKeys.Ascending(article => article.Slug),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "ux_articles_slug"
            });

        await collection.Indexes.CreateOneAsync(index, cancellationToken: cancellationToken);
        logger.LogInformation("Уникальный индекс по адресу статьи готов");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
