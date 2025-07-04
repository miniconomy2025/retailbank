import ErrorPage from "@/pages/ErrorPage";
import { type ReactNode } from "react";
import Loader from "./Loader";

interface PageWrapperProps {
  children?: ReactNode;
  loading: boolean;
  error?: Error | null;
}

function PageWrapper({ children, loading, error }: PageWrapperProps) {
  if (loading) {
    return <Loader />;
  }
  if (error) {
    return <ErrorPage errorMessage={error.message} />;
  }
  return children;
}

export default PageWrapper;
