using Microsoft.Extensions.Logging;
using PersonalBlog.Application.Abstractions.Messaging;
using PersonalBlog.Domain.Articles;
using SharedKernel;

namespace PersonalBlog.Application.Articles;

public sealed record DeleteArticleCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteArticleCommandHandler(
    IArticleRepository repository,
    ILogger<DeleteArticleCommandHandler> logger)
    : ICommandHandler<DeleteArticleCommand, Result>
{
    public async Task<Result> Handle(DeleteArticleCommand command, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(command.Id, cancellationToken);
        if (!deleted)
        {
            return Result.Failure(ArticleErrors.NotFound(command.Id));
        }

        logger.LogInformation("Удалена статья {ArticleId}", command.Id);
        return Result.Success();
    }
}
