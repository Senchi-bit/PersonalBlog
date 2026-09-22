using System.ComponentModel.DataAnnotations;

namespace PersonalBlog.Infrastructure.Persistence.Mongo;

public sealed class PersonalBlogDatabaseSettings
{
    public const string SectionName = "MongoDatabase";

    [Required]
    public string ConnectionString { get; set; } = null!;

    [Required]
    public string DatabaseName { get; set; } = null!;

    [Required]
    public string ArticlesCollectionName { get; set; } = null!;
}
