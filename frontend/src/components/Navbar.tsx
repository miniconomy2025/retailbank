import {
  NavigationMenu,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
} from "@/components/ui/navigation-menu";
import { Link } from "react-router-dom";

function Navbar() {
  return (
    <div className="w-full h-12 flex justify-end pr-4 items-center bg-white shadow">
      <NavigationMenu>
        <NavigationMenuList className="flex gap-4">
          <NavigationMenuItem>
            <NavigationMenuLink asChild>
              <Link
                to="/account/123"
                className="text-sm font-medium text-black hover:underline px-3 py-2"
              >
                Accounts
              </Link>
            </NavigationMenuLink>
          </NavigationMenuItem>
          <NavigationMenuItem>
            <NavigationMenuLink asChild>
              <Link
                to="/"
                className="text-sm font-medium text-black hover:underline px-3 py-2"
              >
                Dashboard
              </Link>
            </NavigationMenuLink>
          </NavigationMenuItem>
        </NavigationMenuList>
      </NavigationMenu>
    </div>
  );
}
export default Navbar;
