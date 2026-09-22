import type { ArticleStatus } from "../api/types";

export function formatDate(value: string | null): string {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(date);
}

export function statusLabel(status: ArticleStatus): string {
  return status === "Published" ? "Опубликовано" : "Черновик";
}
