namespace SharedKernel;

public sealed record ValidationError(Error[] Errors) : Error("Validation.General",
    "Произошла одна или несколько ошибок проверки",
    ErrorType.Validation)
{
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
