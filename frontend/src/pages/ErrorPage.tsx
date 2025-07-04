import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { AlertTriangle } from "lucide-react";

interface ErrorPageProps {
  errorMessage?: string;
}

export default function ErrorPage({
  errorMessage = "Something went wrong. Please try again.",
}: ErrorPageProps) {
  return (
    <div className="h-full w-full flex items-center justify-center  p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-destructive/10">
            <AlertTriangle className="h-8 w-8 text-destructive" />
          </div>
          <CardTitle className="text-2xl font-bold">
            Oops! An error occurred
          </CardTitle>
          <CardDescription>{errorMessage}</CardDescription>
        </CardHeader>
        <CardContent>
          <Button onClick={() => window.location.reload()} className="w-full">
            Refresh Page
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
