using PersonalBlog.Application.Abstractions.Messaging;
using PersonalBlog.Domain.Articles;
using SharedKernel;

namespace PersonalBlog.Application.Articles;

public sealed record GetArticleBySlugQuery(string Slug) : IQuery<Result<ArticleDetails>>;

public sealed class GetArticleBySlugQueryHandler(IArticleRepository repository)
    : IQueryHandler<GetArticleBySlugQuery, Result<ArticleDetails>>
{
    public async Task<Result<ArticleDetails>> Handle(GetArticleBySlugQuery query,
        CancellationToken cancellationToken)
    {
        var article = await repository.GetBySlugAsync(query.Slug, cancellationToken);
        if (article is null || article.Status != ArticleStatus.Published)
        {
            return Result.Failure<ArticleDetails>(ArticleErrors.NotFoundBySlug(query.Slug));
        }

        return ArticleMappings.ToDetails(article);
    }
}
