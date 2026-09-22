using System.Text.RegularExpressions;
using SharedKernel;

namespace PersonalBlog.Domain.Articles;

public sealed class Article
{
    private const int MaxTitleLength = 200;
    private const int MaxSlugLength = 200;
    private const int MaxSummaryLength = 500;
    private const int MaxTagLength = 50;
    private const int MaxTagCount = 20;

    private static readonly Regex SlugPattern = new("^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly List<string> _tags;

    private Article(Guid id, string title, string slug, string? summary, string content, ArticleStatus status,
        List<string> tags, DateTime createdAtUtc, DateTime? updatedAtUtc, DateTime? publishedAtUtc)
    {
        Id = id;
        Title = title;
        Slug = slug;
        Summary = summary;
        Content = content;
        Status = status;
        _tags = tags;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        PublishedAtUtc = publishedAtUtc;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string Slug { get; private set; }

    public string? Summary { get; private set; }

    public string Content { get; private set; }

    public ArticleStatus Status { get; private set; }

    public IReadOnlyList<string> Tags => _tags;

    public DateTime CreatedAtUtc { get; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public static Result<Article> Create(string title, string slug, string? summary, string content,
        IEnumerable<string>? tags, DateTime utcNow)
    {
        var normalized = Normalize(title, slug, summary, content, tags);
        if (normalized.IsFailure)
        {
            return Result.Failure<Article>(normalized.Error);
        }

        var value = normalized.Value;
        return new Article(Guid.NewGuid(), value.Title, value.Slug, value.Summary, value.Content, 
            ArticleStatus.Draft, value.Tags, utcNow, null, null);
    }

    public Result Update(string title, string slug, string? summary, string content, IEnumerable<string>? tags, 
        DateTime utcNow)
    {
        var normalized = Normalize(title, slug, summary, content, tags);
        if (normalized.IsFailure)
        {
            return normalized;
        }

        var value = normalized.Value;
        Title = value.Title;
        Slug = value.Slug;
        Summary = value.Summary;
        Content = value.Content;
        _tags.Clear();
        _tags.AddRange(value.Tags);
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    public Result Publish(DateTime utcNow)
    {
        if (Status == ArticleStatus.Published)
        {
            return Result.Success();
        }

        Status = ArticleStatus.Published;
        PublishedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    public Result Unpublish(DateTime utcNow)
    {
        if (Status == ArticleStatus.Draft)
        {
            return Result.Success();
        }

        Status = ArticleStatus.Draft;
        PublishedAtUtc = null;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    internal static Article Restore(Guid id, string title, string slug, string? summary, string content, ArticleStatus status,
        IEnumerable<string> tags, DateTime createdAtUtc, DateTime? updatedAtUtc, DateTime? publishedAtUtc) =>
        new(id, title, slug, summary, content, status, tags.ToList(), createdAtUtc, updatedAtUtc, publishedAtUtc);

    private static Result<ArticleContent> Normalize(string title, string slug, string? summary, string content,
        IEnumerable<string>? tags)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<ArticleContent>(ArticleErrors.TitleRequired);
        }

        var normalizedTitle = title.Trim();
        if (normalizedTitle.Length > MaxTitleLength)
        {
            return Result.Failure<ArticleContent>(ArticleErrors.TitleTooLong);
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<ArticleContent>(ArticleErrors.SlugRequired);
        }

        var normalizedSlug = slug.Trim();
        if (normalizedSlug.Length > MaxSlugLength || !SlugPattern.IsMatch(normalizedSlug))
        {
            return Result.Failure<ArticleContent>(ArticleErrors.InvalidSlug);
        }

        var normalizedSummary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        if (normalizedSummary is not null && normalizedSummary.Length > MaxSummaryLength)
        {
            return Result.Failure<ArticleContent>(ArticleErrors.SummaryTooLong);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<ArticleContent>(ArticleErrors.ContentRequired);
        }

        var normalizedTags = NormalizeTags(tags);
        if (normalizedTags.IsFailure)
        {
            return Result.Failure<ArticleContent>(normalizedTags.Error);
        }

        return new ArticleContent(
            normalizedTitle,
            normalizedSlug,
            normalizedSummary,
            content.Trim(),
            normalizedTags.Value);
    }

    private static Result<List<string>> NormalizeTags(IEnumerable<string>? tags)
    {
        var normalized = new List<string>();
        if (tags is null)
        {
            return normalized;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            var value = tag.Trim().ToLowerInvariant();
            if (value.Length > MaxTagLength || value.Any(char.IsWhiteSpace))
            {
                return Result.Failure<List<string>>(ArticleErrors.InvalidTag);
            }

            if (seen.Add(value))
            {
                normalized.Add(value);
            }
        }

        if (normalized.Count > MaxTagCount)
        {
            return Result.Failure<List<string>>(ArticleErrors.TooManyTags);
        }

        return normalized;
    }

    private sealed record ArticleContent(
        string Title,
        string Slug,
        string? Summary,
        string Content,
        List<string> Tags);
}
