using PersonalBlog.Application.Abstractions.Messaging;
using PersonalBlog.Domain.Articles;
using SharedKernel;

namespace PersonalBlog.Application.Articles;

public sealed record GetArticleByIdQuery(Guid Id) : IQuery<Result<ArticleDetails>>;

public sealed class GetArticleByIdQueryHandler(IArticleRepository repository)
    : IQueryHandler<GetArticleByIdQuery, Result<ArticleDetails>>
{
    public async Task<Result<ArticleDetails>> Handle(
        GetArticleByIdQuery query,
        CancellationToken cancellationToken)
    {
        var article = await repository.GetByIdAsync(query.Id, cancellationToken);
        return article is null ? Result.Failure<ArticleDetails>(ArticleErrors.NotFound(query.Id)) 
            : ArticleMappings.ToDetails(article);
    }
}
