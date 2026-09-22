using PersonalBlog.Application.Abstractions.Messaging;
using PersonalBlog.Domain.Articles;

namespace PersonalBlog.Application.Articles;

public sealed record ListPublishedArticlesQuery : IQuery<IReadOnlyList<ArticleListItem>>;

public sealed class ListPublishedArticlesQueryHandler(IArticleRepository repository)
    : IQueryHandler<ListPublishedArticlesQuery, IReadOnlyList<ArticleListItem>>
{
    public async Task<IReadOnlyList<ArticleListItem>> Handle(ListPublishedArticlesQuery query,
        CancellationToken cancellationToken)
    {
        var articles = await repository.ListPublishedAsync(cancellationToken);
        return articles.Select(ArticleMappings.ToListItem).ToArray();
    }
}
