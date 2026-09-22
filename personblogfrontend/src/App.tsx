import { BrowserRouter, Route, Routes } from "react-router-dom";
import { Layout } from "./components/Layout";
import { ArticlePage } from "./pages/ArticlePage";
import { EditorPage, WritePage } from "./pages/EditorPage";
import { HomePage } from "./pages/HomePage";

export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<HomePage />} />
          <Route path="articles/:slug" element={<ArticlePage />} />
          <Route path="write" element={<WritePage />} />
          <Route path="write/:id" element={<EditorPage />} />
          <Route path="*" element={<p className="status">Страница не найдена.</p>} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
