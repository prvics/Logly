export type ThemeMode = "light" | "dark";

export type ThemeColors = {
    background: string;
    surface: string;
    surfaceMuted: string;
    border: string;
    textPrimary: string;
    textSecondary: string;
    primary: string;
    primaryText: string;
    danger: string;
    success: string;
    skeleton: string;
    switchTrackOff: string;
    switchTrackOn: string;
    switchThumb: string;
};

export const lightTheme: ThemeColors = {
    background: "#F6F7FB",
    surface: "#FFFFFF",
    surfaceMuted: "#E7E9F0",
    border: "#E4E7F0",
    textPrimary: "#1B1D28",
    textSecondary: "#666B7A",
    primary: "#365CF5",
    primaryText: "#FFFFFF",
    danger: "#D92D20",
    success: "#12B76A",
    skeleton: "#E2E5EE",
    switchTrackOff: "#D6DAE5",
    switchTrackOn: "#365CF5",
    switchThumb: "#FFFFFF",
};

export const darkTheme: ThemeColors = {
    background: "#0F1117",
    surface: "#171A22",
    surfaceMuted: "#232733",
    border: "#2A2F3B",
    textPrimary: "#F3F5FA",
    textSecondary: "#A2A9BB",
    primary: "#5B7CFF",
    primaryText: "#FFFFFF",
    danger: "#FF6369",
    success: "#3DD68C",
    skeleton: "#2A2F3B",
    switchTrackOff: "#333947",
    switchTrackOn: "#5B7CFF",
    switchThumb: "#FFFFFF",
};

export const themeRegistry = {
    light: lightTheme,
    dark: darkTheme,
} as const;

export type ThemeName = keyof typeof themeRegistry;