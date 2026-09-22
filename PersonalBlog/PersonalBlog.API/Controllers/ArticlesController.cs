using Microsoft.AspNetCore.Mvc;
using PersonalBlog.Application.Abstractions.Messaging;
using PersonalBlog.Application.Articles;
using PersonalBlog.Infrastructure;
using SharedKernel;

namespace PersonalBlog.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ArticlesController(ICommandDispatcher commands, IQueryDispatcher queries) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var articles = await queries
            .Dispatch<ListPublishedArticlesQuery, IReadOnlyList<ArticleListItem>>(new ListPublishedArticlesQuery(), cancellationToken);
        return Ok(articles);
    }

    [HttpGet("Article")]
    public async Task<IActionResult> GetById([FromQuery] GetArticleByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await queries.Dispatch<GetArticleByIdQuery, Result<ArticleDetails>>(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpGet("ArticleBySlug")]
    public async Task<IActionResult> GetBySlug([FromQuery] GetArticleBySlugQuery query, CancellationToken cancellationToken)
    {
        var result = await queries.Dispatch<GetArticleBySlugQuery, Result<ArticleDetails>>(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpPost("Article")]
    public async Task<IActionResult> Create([FromBody] CreateArticleCommand command, CancellationToken cancellationToken)
    {
        var result = await commands.Dispatch<CreateArticleCommand, Result<Guid>>(command, cancellationToken);
        if (result.IsFailure)
        {
            return CustomResults.Problem(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpPut("Article")]
    public async Task<IActionResult> Update([FromBody] UpdateArticleCommand command, CancellationToken cancellationToken)
    {
        var result = await commands.Dispatch<UpdateArticleCommand, Result<ArticleDetails>>(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpDelete("Article")]
    public async Task<IActionResult> Delete([FromBody] DeleteArticleCommand command, CancellationToken cancellationToken)
    {
        var result = await commands.Dispatch<DeleteArticleCommand, Result>(command, cancellationToken);
        return result.IsSuccess ? NoContent() : CustomResults.Problem(result);
    }
}
