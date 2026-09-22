export type ArticleStatus = "Draft" | "Published";

export type ArticleListItem = {
  id: string;
  title: string;
  slug: string;
  summary: string | null;
  tags: string[];
  publishedAtUtc: string | null;
};

export type ArticleDetails = {
  id: string;
  title: string;
  slug: string;
  summary: string | null;
  content: string;
  tags: string[];
  status: ArticleStatus;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  publishedAtUtc: string | null;
};

export type ArticleInput = {
  title: string;
  slug: string;
  summary: string | null;
  content: string;
  tags: string[];
};

export type UpdateArticleInput = ArticleInput & {
  id: string;
  status: ArticleStatus;
};
