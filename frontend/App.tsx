import { StatusBar } from "expo-status-bar";
import { AppNavigator } from "./src/navigation/AppNavigator";
import {
  ThemeProvider,
  useTheme,
} from "./src/theme/ThemeProvider";

function AppStatusBar() {
  const { mode } = useTheme();

  return (
    <StatusBar
      style={mode === "dark" ? "light" : "dark"}
    />
  );
}

export default function App() {
  return (
    <ThemeProvider>
      <AppStatusBar />
      <AppNavigator />
    </ThemeProvider>
  );
}