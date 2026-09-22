using PersonalBlog.Domain.Articles;

namespace PersonalBlog.Application.Articles;

public sealed record ArticleDetails(Guid Id, string Title, string Slug, string? Summary, string Content,
    IReadOnlyList<string> Tags, ArticleStatus Status, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc, 
    DateTime? PublishedAtUtc);

public sealed record ArticleListItem(Guid Id, string Title, string Slug, string? Summary,
    IReadOnlyList<string> Tags, DateTime? PublishedAtUtc);

internal static class ArticleMappings
{
    public static ArticleDetails ToDetails(Article article) =>
        new(article.Id, article.Title, article.Slug, article.Summary, article.Content, article.Tags,
            article.Status, article.CreatedAtUtc, article.UpdatedAtUtc, article.PublishedAtUtc);

    public static ArticleListItem ToListItem(Article article) =>
        new(article.Id, article.Title, article.Slug, article.Summary, article.Tags, article.PublishedAtUtc);
}
