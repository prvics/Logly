import {
    createContext,
    useCallback,
    useContext,
    useEffect,
    useMemo,
    useState,
    type ReactNode,
} from "react";
import AsyncStorage from "@react-native-async-storage/async-storage";

import {
    themeRegistry,
    type ThemeColors,
    type ThemeMode,
    type ThemeName,
} from "./themes";

const THEME_STORAGE_KEY = "logly.theme";

type ThemeContextValue = {
    themeName: ThemeName;
    mode: ThemeMode;
    colors: ThemeColors;
    setTheme: (themeName: ThemeName) => void;
    toggleMode: () => void;
};

const ThemeContext = createContext<ThemeContextValue | null>(null);

export function ThemeProvider({
    children,
}: {
    children: ReactNode;
}) {
    const [themeName, setThemeName] =
        useState<ThemeName>("light");
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        let isMounted = true;

        async function loadTheme() {
            try {
                const storedTheme = await AsyncStorage.getItem(
                    THEME_STORAGE_KEY
                );

                if (
                    isMounted &&
                    storedTheme &&
                    storedTheme in themeRegistry
                ) {
                    setThemeName(storedTheme as ThemeName);
                }
            } catch {
                // Ha nem sikerül betölteni, marad az alapértelmezett téma.
            } finally {
                if (isMounted) {
                    setIsLoaded(true);
                }
            }
        }

        void loadTheme();

        return () => {
            isMounted = false;
        };
    }, []);

    const setTheme = useCallback((nextTheme: ThemeName) => {
        setThemeName(nextTheme);

        void AsyncStorage.setItem(
            THEME_STORAGE_KEY,
            nextTheme
        ).catch(() => {
            // A téma mentési hibája nem törheti meg az appot.
        });
    }, []);

    const toggleMode = useCallback(() => {
        setTheme(
            themeName === "dark" ? "light" : "dark"
        );
    }, [themeName, setTheme]);

    const value = useMemo<ThemeContextValue>(
        () => ({
            themeName,
            mode: themeName === "dark" ? "dark" : "light",
            colors: themeRegistry[themeName],
            setTheme,
            toggleMode,
        }),
        [themeName, setTheme, toggleMode]
    );

    if (!isLoaded) {
        return null;
    }

    return (
        <ThemeContext.Provider value={value}>
            {children}
        </ThemeContext.Provider>
    );
}

export function useTheme() {
    const context = useContext(ThemeContext);

    if (!context) {
        throw new Error(
            "A useTheme csak ThemeProvider-en belül használható."
        );
    }

    return context;
}