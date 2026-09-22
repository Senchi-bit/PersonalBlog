import type { ArticleStatus } from "../api/types";

const slugPattern = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;

export type ArticleFormValues = {
  title: string;
  slug: string;
  summary: string;
  content: string;
  tags: string;
  status: ArticleStatus;
};

export const emptyArticleForm: ArticleFormValues = {
  title: "",
  slug: "",
  summary: "",
  content: "",
  tags: "",
  status: "Draft",
};

export function parseTags(raw: string): string[] {
  const seen = new Set<string>();
  const tags: string[] = [];

  for (const part of raw.split(/[,\s]+/)) {
    const tag = part.trim().toLowerCase();
    if (!tag || seen.has(tag)) {
      continue;
    }

    seen.add(tag);
    tags.push(tag);
  }

  return tags;
}

export function validateArticle(form: ArticleFormValues): string | null {
  const title = form.title.trim();
  if (!title) {
    return "Укажите заголовок.";
  }
  if (title.length > 200) {
    return "Заголовок должен быть не длиннее 200 символов.";
  }

  const slug = form.slug.trim();
  if (!slug) {
    return "Укажите адрес статьи.";
  }
  if (slug.length > 200 || !slugPattern.test(slug)) {
    return "Адрес статьи должен состоять из строчных латинских букв, цифр и дефисов. А также не более 200 символов";
  }

  if (form.summary.trim().length > 500) {
    return "Краткое описание должно быть не длиннее 500 символов.";
  }

  if (!form.content.trim()) {
    return "Укажите текст статьи.";
  }

  const tags = parseTags(form.tags);
  if (tags.length > 20) {
    return "У статьи может быть не больше 20 тегов.";
  }
  if (tags.some((tag) => tag.length > 50)) {
    return "Каждый тег должен быть одним словом не длиннее 50 символов.";
  }

  return null;
}
