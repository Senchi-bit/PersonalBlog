import { useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  ApiError,
  createArticle,
  deleteArticle,
  getArticleById,
  updateArticle,
} from "../api/articles";
import type { ArticleDetails, ArticleStatus } from "../api/types";
import { MyArticles } from "../components/MyArticles";
import {
  emptyArticleForm,
  parseTags,
  validateArticle,
  type ArticleFormValues,
} from "../lib/articleForm";
import { statusLabel } from "../lib/format";
import { forgetArticleId, rememberArticleId } from "../lib/savedArticles";
import { slugFromTitle } from "../lib/slug";

export function WritePage() {
  return (
    <>
      <MyArticles />
      <ArticleForm mode="create" />
    </>
  );
}

export function EditorPage() {
  const { id = "" } = useParams();
  const [article, setArticle] = useState<ArticleDetails | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    setArticle(null);

    getArticleById(id)
      .then((item) => {
        if (cancelled) {
          return;
        }
        rememberArticleId(item.id);
        setArticle(item);
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
  }, [id]);

  if (loading) {
    return <p className="status">Загрузка статьи…</p>;
  }

  if (error || !article) {
    return <p className="status error">{error ?? "Статья не найдена."}</p>;
  }

  return <ArticleForm key={article.id} mode="edit" article={article} />;
}

function ArticleForm({
  mode,
  article,
}: {
  mode: "create" | "edit";
  article?: ArticleDetails;
}) {
  const navigate = useNavigate();
  const [form, setForm] = useState<ArticleFormValues>(() =>
    article
      ? {
          title: article.title,
          slug: article.slug,
          summary: article.summary ?? "",
          content: article.content,
          tags: article.tags.join(", "),
          status: article.status,
        }
      : emptyArticleForm,
  );
  const [slugTouched, setSlugTouched] = useState(mode === "edit");
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [saved, setSaved] = useState<ArticleDetails | null>(article ?? null);

  function updateField<K extends keyof ArticleFormValues>(key: K, value: ArticleFormValues[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  function onTitleChange(title: string) {
    setForm((current) => ({
      ...current,
      title,
      slug: slugTouched ? current.slug : slugFromTitle(title),
    }));
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setNotice(null);
    const validationError = validateArticle(form);
    if (validationError) {
      setError(validationError);
      return;
    }

    const input = {
      title: form.title.trim(),
      slug: form.slug.trim(),
      summary: form.summary.trim() ? form.summary.trim() : null,
      content: form.content.trim(),
      tags: parseTags(form.tags),
    };

    setSaving(true);
    setError(null);
    try {
      if (mode === "create") {
        const created = await createArticle(input);
        rememberArticleId(created.id);
        navigate(`/write/${created.id}`);
        return;
      }

      if (!article) {
        return;
      }

      const updated = await updateArticle({ ...input, id: article.id, status: form.status });
      rememberArticleId(updated.id);
      setSaved(updated);
      setForm({
        title: updated.title,
        slug: updated.slug,
        summary: updated.summary ?? "",
        content: updated.content,
        tags: updated.tags.join(", "),
        status: updated.status,
      });
      setNotice("Сохранено.");
    } catch (reason: unknown) {
      setError(reason instanceof ApiError ? reason.message : "Не удалось сохранить статью.");
    } finally {
      setSaving(false);
    }
  }

  async function onDelete() {
    if (!article) {
      return;
    }

    setSaving(true);
    setError(null);
    try {
      await deleteArticle(article.id);
      forgetArticleId(article.id);
      navigate("/write");
    } catch (reason: unknown) {
      setError(reason instanceof ApiError ? reason.message : "Не удалось удалить статью.");
      setSaving(false);
    }
  }

  const publicSlug = saved?.status === "Published" ? saved.slug : null;

  return (
    <section className="editor">
      <h1>{mode === "create" ? "Новая статья" : "Редактирование"}</h1>
      {mode === "edit" ? (
        <p className="editor-links">
          <Link to="/write">К моим статьям</Link>
          {publicSlug ? <Link to={`/articles/${publicSlug}`}>Открыть на сайте</Link> : null}
        </p>
      ) : null}
      <form onSubmit={onSubmit}>
        <label>
          Заголовок
          <input
            value={form.title}
            maxLength={200}
            onChange={(event) => onTitleChange(event.target.value)}
          />
        </label>
        <label>
          Адрес
          <input
            value={form.slug}
            maxLength={200}
            spellCheck={false}
            onChange={(event) => {
              setSlugTouched(true);
              updateField("slug", event.target.value);
            }}
          />
          <span className="hint">Строчные латинские буквы, цифры и дефисы.</span>
        </label>
        <label>
          Краткое описание
          <textarea
            value={form.summary}
            maxLength={500}
            rows={3}
            onChange={(event) => updateField("summary", event.target.value)}
          />
        </label>
        <label>
          Текст
          <textarea
            value={form.content}
            rows={14}
            onChange={(event) => updateField("content", event.target.value)}
          />
          <span className="hint">Поддерживается Markdown.</span>
        </label>
        <label>
          Теги
          <input
            value={form.tags}
            onChange={(event) => updateField("tags", event.target.value)}
          />
          <span className="hint">Через запятую, одно слово на тег, не больше 20.</span>
        </label>
        {mode === "edit" ? (
          <fieldset>
            <legend>Статус</legend>
            <div className="status-toggle">
              {(["Draft", "Published"] as ArticleStatus[]).map((status) => (
                <label key={status}>
                  <input
                    type="radio"
                    name="status"
                    checked={form.status === status}
                    onChange={() => updateField("status", status)}
                  />
                  {statusLabel(status)}
                </label>
              ))}
            </div>
          </fieldset>
        ) : (
          <p className="hint">Новая статья сохраняется как черновик. Опубликовать её можно после сохранения.</p>
        )}
        {error ? <p className="status error">{error}</p> : null}
        {notice ? <p className="status notice">{notice}</p> : null}
        <div className="form-actions">
          <button type="submit" disabled={saving}>
            {saving ? "Сохранение…" : "Сохранить"}
          </button>
          {mode === "edit" && !confirmDelete ? (
            <button type="button" className="danger" disabled={saving} onClick={() => setConfirmDelete(true)}>
              Удалить
            </button>
          ) : null}
          {mode === "edit" && confirmDelete ? (
            <>
              <span>Удалить статью?</span>
              <button type="button" className="danger" disabled={saving} onClick={() => void onDelete()}>
                Да, удалить
              </button>
              <button type="button" disabled={saving} onClick={() => setConfirmDelete(false)}>
                Отмена
              </button>
            </>
          ) : null}
        </div>
      </form>
    </section>
  );
}
