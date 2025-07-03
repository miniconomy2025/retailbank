import { type ReactNode } from "react";

interface PageWrapperProps {
  children?: ReactNode;
  loading: boolean;
  error?: Error | null;
}

function PageWrapper({ children, loading, error }: PageWrapperProps) {
  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="w-6 h-6 border-2 border-gray-300 border-t-gray-800 rounded-full animate-spin" />
      </div>
    );
  }

  if (error) {
    return (
      <h1 className="h-full w-full flex items-center justify-center text-4xl">
        Error: {error.message}
      </h1>
    );
  }

  return <div>{children}</div>;
}

export default PageWrapper;
