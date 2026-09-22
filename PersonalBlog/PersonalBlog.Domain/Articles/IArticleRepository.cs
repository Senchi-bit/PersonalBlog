using SharedKernel;

namespace PersonalBlog.Domain.Articles;

public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<Article>> ListPublishedAsync(CancellationToken cancellationToken);

    Task<Result> AddAsync(Article article, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Article article, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken);
}
