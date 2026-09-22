using PersonalBlog.Domain.Articles;
using PersonalBlog.Infrastructure.Persistence.Mongo.Documents;

namespace PersonalBlog.Infrastructure.Persistence.Mongo;

internal static class ArticleMapper
{
    public static Article ToDomain(ArticleDocument document) =>
        Article.Restore(document.Id, document.Title, document.Slug, document.Summary, document.Content,
            document.Status, document.Tags, document.CreatedAtUtc, document.UpdatedAtUtc, document.PublishedAtUtc);

    public static ArticleDocument ToDocument(Article article) =>
        new()
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            Content = article.Content,
            Status = article.Status,
            Tags = article.Tags.ToList(),
            CreatedAtUtc = article.CreatedAtUtc,
            UpdatedAtUtc = article.UpdatedAtUtc,
            PublishedAtUtc = article.PublishedAtUtc
        };
}
