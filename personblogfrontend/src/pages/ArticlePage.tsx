import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import { ApiError, getArticleBySlug } from "../api/articles";
import type { ArticleDetails } from "../api/types";
import { formatDate } from "../lib/format";

export function ArticlePage() {
  const { slug = "" } = useParams();
  const [article, setArticle] = useState<ArticleDetails | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    setArticle(null);

    getArticleBySlug(slug)
      .then((item) => {
        if (!cancelled) {
          setArticle(item);
        }
      })
      .catch((reason: unknown) => {
        if (cancelled) {
          return;
        }
        if (reason instanceof ApiError && reason.status === 404) {
          setError("Статья не найдена.");
          return;
        }
        setError(reason instanceof ApiError ? reason.message : "Не удалось загрузить статью.");
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [slug]);

  if (loading) {
    return <p className="status">Загрузка статьи…</p>;
  }

  if (error || !article) {
    return <p className="status error">{error ?? "Статья не найдена."}</p>;
  }

  return (
    <article className="article">
      <p className="meta">{formatDate(article.publishedAtUtc)}</p>
      <h1>{article.title}</h1>
      {article.summary ? <p className="summary">{article.summary}</p> : null}
      {article.tags.length > 0 ? (
        <ul className="tags">
          {article.tags.map((tag) => (
            <li key={tag}>{tag}</li>
          ))}
        </ul>
      ) : null}
      <div className="prose">
        <ReactMarkdown>{article.content}</ReactMarkdown>
      </div>
      <p className="article-actions">
        <Link to={`/write/${article.id}`}>Редактировать</Link>
      </p>
    </article>
  );
}
