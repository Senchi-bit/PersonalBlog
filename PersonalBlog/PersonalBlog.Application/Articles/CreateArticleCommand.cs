using Microsoft.Extensions.Logging;
using PersonalBlog.Application.Abstractions.Messaging;
using PersonalBlog.Domain.Articles;
using SharedKernel;

namespace PersonalBlog.Application.Articles;

public sealed record CreateArticleCommand(string Title, string Slug, string? Summary, string Content, 
    IReadOnlyList<string>? Tags) : ICommand<Result<Guid>>;

public sealed class CreateArticleCommandHandler(IArticleRepository repository,
    IDateTimeProvider dateTimeProvider,
    ILogger<CreateArticleCommandHandler> logger)
    : ICommandHandler<CreateArticleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateArticleCommand command, CancellationToken cancellationToken)
    {
        var created = Article.Create(command.Title, command.Slug, command.Summary, command.Content, 
            command.Tags, dateTimeProvider.UtcNow);

        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        var article = created.Value;
        if (await repository.SlugExistsAsync(article.Slug, null, cancellationToken))
        {
            return Result.Failure<Guid>(ArticleErrors.SlugConflict(article.Slug));
        }

        var saved = await repository.AddAsync(article, cancellationToken);
        if (saved.IsFailure)
        {
            return Result.Failure<Guid>(saved.Error);
        }

        logger.LogInformation("Создана статья {ArticleId} с адресом {Slug}", article.Id, article.Slug);
        return article.Id;
    }
}
