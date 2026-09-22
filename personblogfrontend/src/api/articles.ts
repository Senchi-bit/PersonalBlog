import type { ArticleDetails, ArticleInput, ArticleListItem, UpdateArticleInput } from "./types";

export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response;
  try {
    response = await fetch(path, {
      ...init,
      headers: {
        Accept: "application/json",
        ...(init?.body ? { "Content-Type": "application/json" } : {}),
        ...init?.headers,
      },
    });
  } catch {
    throw new ApiError("Не удалось связаться с сервером.", 0);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  if (!response.ok) {
    throw new ApiError(await readProblem(response), response.status);
  }

  return (await response.json()) as T;
}

async function readProblem(response: Response): Promise<string> {
  const fallback = "Не удалось выполнить запрос.";
  const text = await response.text();
  if (!text) {
    return fallback;
  }

  try {
    const problem = JSON.parse(text) as { detail?: unknown; title?: unknown };
    if (typeof problem.detail === "string" && problem.detail) {
      return problem.detail;
    }
    if (typeof problem.title === "string" && problem.title) {
      return problem.title;
    }
  } catch {
    return fallback;
  }

  return fallback;
}

export function listArticles() {
  return request<ArticleListItem[]>("/api/Articles");
}

export function getArticleBySlug(slug: string) {
  const params = new URLSearchParams({ slug });
  return request<ArticleDetails>(`/api/Articles/ArticleBySlug?${params}`);
}

export function getArticleById(id: string) {
  const params = new URLSearchParams({ id });
  return request<ArticleDetails>(`/api/Articles/Article?${params}`);
}

export function createArticle(input: ArticleInput) {
  return request<{ id: string }>("/api/Articles/Article", {
    method: "POST",
    body: JSON.stringify(input),
  });
}

export function updateArticle(input: UpdateArticleInput) {
  return request<ArticleDetails>("/api/Articles/Article", {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export function deleteArticle(id: string) {
  return request<void>("/api/Articles/Article", {
    method: "DELETE",
    body: JSON.stringify({ id }),
  });
}
