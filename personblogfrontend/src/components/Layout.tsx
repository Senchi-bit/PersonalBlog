import { NavLink, Outlet } from "react-router-dom";

export function Layout() {
  return (
    <div className="site">
      <header className="site-header">
        <NavLink to="/" end className="brand">
          Блог
        </NavLink>
        <nav>
          <NavLink to="/write">Написать</NavLink>
        </nav>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
}
