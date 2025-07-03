import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function NotFound() {
  const navigate = useNavigate();

  return (
    <div className="h-full w-full flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl font-bold">Page not found</CardTitle>
        </CardHeader>
        <CardContent>
          <Button onClick={() => navigate("/")} className="w-full">
            Go to Overview
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}