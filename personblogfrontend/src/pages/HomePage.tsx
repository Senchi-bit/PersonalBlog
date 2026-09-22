import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { ApiError, listArticles } from "../api/articles";
import type { ArticleListItem } from "../api/types";
import { formatDate } from "../lib/format";

export function HomePage() {
  const [articles, setArticles] = useState<ArticleListItem[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    listArticles()
      .then((items) => {
        if (!cancelled) {
          setArticles(items);
        }
      })
      .catch((reason: unknown) => {
        if (!cancelled) {
          setError(reason instanceof ApiError ? reason.message : "Не удалось загрузить статьи.");
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  if (loading) {
    return <p className="status">Загрузка статей…</p>;
  }

  if (error) {
    return <p className="status error">{error}</p>;
  }

  if (articles.length === 0) {
    return (
      <section className="empty">
        <h1>Пока пусто</h1>
        <p>Опубликованных статей ещё нет.</p>
      </section>
    );
  }

  return (
    <section className="feed">
      <h1>Заметки</h1>
      <ul className="article-list">
        {articles.map((article) => (
          <li key={article.id}>
            <article>
              <p className="meta">{formatDate(article.publishedAtUtc)}</p>
              <h2>
                <Link to={`/articles/${article.slug}`}>{article.title}</Link>
              </h2>
              {article.summary ? <p>{article.summary}</p> : null}
              {article.tags.length > 0 ? (
                <ul className="tags">
                  {article.tags.map((tag) => (
                    <li key={tag}>{tag}</li>
                  ))}
                </ul>
              ) : null}
            </article>
          </li>
        ))}
      </ul>
    </section>
  );
}
