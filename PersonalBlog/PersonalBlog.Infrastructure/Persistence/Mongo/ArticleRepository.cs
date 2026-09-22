using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalBlog.Domain.Articles;
using PersonalBlog.Infrastructure.Persistence.Mongo.Documents;
using SharedKernel;

namespace PersonalBlog.Infrastructure.Persistence.Mongo;

internal sealed class ArticleRepository(IMongoDatabase database,
    IOptions<PersonalBlogDatabaseSettings> settings) : IArticleRepository
{
    private readonly IMongoCollection<ArticleDocument> _articles =
        database.GetCollection<ArticleDocument>(settings.Value.ArticlesCollectionName);

    public async Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _articles.Find(article => article.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ArticleMapper.ToDomain(document);
    }

    public async Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var document = await _articles.Find(article => article.Slug == slug).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ArticleMapper.ToDomain(document);
    }

    public async Task<IReadOnlyList<Article>> ListPublishedAsync(CancellationToken cancellationToken)
    {
        var filter = Builders<ArticleDocument>.Filter.Eq(article => article.Status, ArticleStatus.Published);
        var documents = await _articles
            .Find(filter)
            .SortByDescending(article => article.PublishedAtUtc)
            .ToListAsync(cancellationToken);

        return documents.Select(ArticleMapper.ToDomain).ToArray();
    }

    public async Task<Result> AddAsync(Article article, CancellationToken cancellationToken)
    {
        try
        {
            await _articles.InsertOneAsync(ArticleMapper.ToDocument(article), cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (MongoWriteException exception) when (IsDuplicateKey(exception))
        {
            return Result.Failure(ArticleErrors.SlugConflict(article.Slug));
        }
    }

    public async Task<Result> UpdateAsync(Article article, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _articles.ReplaceOneAsync(
                document => document.Id == article.Id,
                ArticleMapper.ToDocument(article),
                cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
            {
                return Result.Failure(ArticleErrors.NotFound(article.Id));
            }

            return Result.Success();
        }
        catch (MongoWriteException exception) when (IsDuplicateKey(exception))
        {
            return Result.Failure(ArticleErrors.SlugConflict(article.Slug));
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _articles.DeleteOneAsync(article => article.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken)
    {
        var filter = Builders<ArticleDocument>.Filter.Eq(article => article.Slug, slug);
        if (excludingId is Guid id)
        {
            filter &= Builders<ArticleDocument>.Filter.Ne(article => article.Id, id);
        }

        return await _articles.Find(filter).AnyAsync(cancellationToken);
    }

    private static bool IsDuplicateKey(MongoWriteException exception) =>
        exception.WriteError.Category == ServerErrorCategory.DuplicateKey;
}
