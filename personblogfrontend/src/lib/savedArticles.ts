const storageKey = "personblog.articleIds";

export function readSavedArticleIds(): string[] {
  try {
    const raw = localStorage.getItem(storageKey);
    if (!raw) {
      return [];
    }

    const parsed: unknown = JSON.parse(raw);
    if (!Array.isArray(parsed)) {
      return [];
    }

    return parsed.filter((id): id is string => typeof id === "string" && id.length > 0);
  } catch {
    return [];
  }
}

export function rememberArticleId(id: string) {
  const ids = [id, ...readSavedArticleIds().filter((item) => item !== id)];
  localStorage.setItem(storageKey, JSON.stringify(ids));
}

export function forgetArticleId(id: string) {
  const ids = readSavedArticleIds().filter((item) => item !== id);
  localStorage.setItem(storageKey, JSON.stringify(ids));
}
