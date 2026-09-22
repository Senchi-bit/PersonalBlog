import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { ApiError, getArticleById, listArticles } from "../api/articles";
import type { ArticleStatus } from "../api/types";
import { statusLabel } from "../lib/format";
import { forgetArticleId, readSavedArticleIds } from "../lib/savedArticles";

type Mine = {
  id: string;
  title: string;
  status: ArticleStatus;
};

export function MyArticles() {
  const [articles, setArticles] = useState<Mine[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const published = await listArticles();
        const publishedIds = new Set(published.map((article) => article.id));
        const savedIds = readSavedArticleIds().filter((id) => !publishedIds.has(id));
        const drafts = await Promise.all(
          savedIds.map(async (id) => {
            try {
              return await getArticleById(id);
            } catch (reason: unknown) {
              if (reason instanceof ApiError && reason.status === 404) {
                forgetArticleId(id);
              }
              return null;
            }
          }),
        );

        if (cancelled) {
          return;
        }

        const items: Mine[] = [
          ...drafts.flatMap((article) =>
            article
              ? [{ id: article.id, title: article.title, status: article.status }]
              : [],
          ),
          ...published.map((article) => ({
            id: article.id,
            title: article.title,
            status: "Published" as const,
          })),
        ];
        setArticles(items);
      } catch (reason: unknown) {
        if (!cancelled) {
          setError(reason instanceof ApiError ? reason.message : "Не удалось загрузить статьи.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <section className="mine">
      <h1>Мои статьи</h1>
      {loading ? <p className="status">Загрузка…</p> : null}
      {error ? <p className="status error">{error}</p> : null}
      {!loading && !error && articles.length === 0 ? (
        <p className="hint">
          Здесь появятся статьи, созданные в этом браузере, и уже опубликованные.
        </p>
      ) : null}
      {articles.length > 0 ? (
        <ul className="mine-list">
          {articles.map((article) => (
            <li key={article.id}>
              <Link to={`/write/${article.id}`}>{article.title}</Link>
              <span className={`badge ${article.status === "Published" ? "published" : "draft"}`}>
                {statusLabel(article.status)}
              </span>
            </li>
          ))}
        </ul>
      ) : null}
    </section>
  );
}
