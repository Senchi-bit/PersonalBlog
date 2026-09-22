using SharedKernel;

namespace PersonalBlog.Domain.Articles;

public static class ArticleErrors
{
    public static readonly Error TitleRequired = new(
        "Article.TitleRequired",
        "Укажите заголовок.",
        ErrorType.Validation);

    public static readonly Error TitleTooLong = new(
        "Article.TitleTooLong",
        "Заголовок должен быть не длиннее 200 символов.",
        ErrorType.Validation);

    public static readonly Error SlugRequired = new(
        "Article.SlugRequired",
        "Укажите адрес статьи.",
        ErrorType.Validation);

    public static readonly Error InvalidSlug = new(
        "Article.InvalidSlug",
        "Адрес статьи должен состоять из строчных латинских букв, цифр и дефисов.",
        ErrorType.Validation);

    public static readonly Error SummaryTooLong = new(
        "Article.SummaryTooLong",
        "Краткое описание должно быть не длиннее 500 символов.",
        ErrorType.Validation);

    public static readonly Error ContentRequired = new(
        "Article.ContentRequired",
        "Укажите текст статьи.",
        ErrorType.Validation);

    public static readonly Error InvalidTag = new(
        "Article.InvalidTag",
        "Каждый тег должен быть одним словом не длиннее 50 символов.",
        ErrorType.Validation);

    public static readonly Error TooManyTags = new(
        "Article.TooManyTags",
        "У статьи может быть не больше 20 тегов.",
        ErrorType.Validation);

    public static readonly Error InvalidStatus = new(
        "Article.InvalidStatus",
        "Недопустимый статус статьи.",
        ErrorType.Validation);

    public static Error NotFound(Guid id) =>
        Error.NotFound("Article.NotFound", $"Статья '{id}' не найдена.");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Article.NotFound", $"Статья '{slug}' не найдена.");

    public static Error SlugConflict(string slug) =>
        Error.Conflict("Article.SlugConflict", $"Адрес '{slug}' уже занят.");
}
