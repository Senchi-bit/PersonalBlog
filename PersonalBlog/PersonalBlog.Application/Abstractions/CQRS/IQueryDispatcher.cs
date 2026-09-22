namespace PersonalBlog.Application.Abstractions.Messaging;

public interface IQueryDispatcher
{
    Task<TResponse> Dispatch<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken) where TQuery : IQuery<TResponse>;
}
