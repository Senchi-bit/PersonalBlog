using Microsoft.Extensions.Logging;
using PersonalBlog.Application.Abstractions.Messaging;
using PersonalBlog.Domain.Articles;
using SharedKernel;

namespace PersonalBlog.Application.Articles;

public sealed record UpdateArticleRequest(string Title, string Slug, string? Summary, string Content, 
    IReadOnlyList<string>? Tags, ArticleStatus Status);

public sealed record UpdateArticleCommand(Guid Id, string Title, string Slug, string? Summary, string Content,
    IReadOnlyList<string>? Tags, ArticleStatus Status) : ICommand<Result<ArticleDetails>>;

public sealed class UpdateArticleCommandHandler(
    IArticleRepository repository,
    IDateTimeProvider dateTimeProvider,
    ILogger<UpdateArticleCommandHandler> logger)
    : ICommandHandler<UpdateArticleCommand, Result<ArticleDetails>>
{
    public async Task<Result<ArticleDetails>> Handle(
        UpdateArticleCommand command,
        CancellationToken cancellationToken)
    {
        var article = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (article is null)
        {
            return Result.Failure<ArticleDetails>(ArticleErrors.NotFound(command.Id));
        }

        var utcNow = dateTimeProvider.UtcNow;
        var updated = article.Update(
            command.Title,
            command.Slug,
            command.Summary,
            command.Content,
            command.Tags,
            utcNow);

        if (updated.IsFailure)
        {
            return Result.Failure<ArticleDetails>(updated.Error);
        }

        var statusResult = command.Status switch
        {
            ArticleStatus.Published => article.Publish(utcNow),
            ArticleStatus.Draft => article.Unpublish(utcNow),
            _ => Result.Failure(ArticleErrors.InvalidStatus)
        };

        if (statusResult.IsFailure)
        {
            return Result.Failure<ArticleDetails>(statusResult.Error);
        }

        if (await repository.SlugExistsAsync(article.Slug, article.Id, cancellationToken))
        {
            return Result.Failure<ArticleDetails>(ArticleErrors.SlugConflict(article.Slug));
        }

        var saved = await repository.UpdateAsync(article, cancellationToken);
        if (saved.IsFailure)
        {
            return Result.Failure<ArticleDetails>(saved.Error);
        }

        logger.LogInformation("Обновлена статья {ArticleId} с адресом {Slug}", article.Id, article.Slug);
        return ArticleMappings.ToDetails(article);
    }
}
