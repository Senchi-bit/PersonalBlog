using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PersonalBlog.Domain.Articles;

namespace PersonalBlog.Infrastructure.Persistence.Mongo.Documents;

internal sealed class ArticleDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    [BsonRepresentation(BsonType.String)]
    public ArticleStatus Status { get; set; }

    public List<string> Tags { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}
