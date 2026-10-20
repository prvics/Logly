import { useEffect, useMemo } from "react";
import {
  ActivityIndicator,
  StyleSheet,
  View,
} from "react-native";
import {
  DarkTheme,
  DefaultTheme,
  NavigationContainer,
  type Theme,
} from "@react-navigation/native";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";

import { LoginScreen } from "../screens/LoginScreen";
import { RegisterScreen } from "../screens/RegisterScreen";
import {
  ForgotPasswordScreen,
  ResetPasswordScreen,
} from "../screens/PasswordResetScreens";
import { DashboardScreen } from "../screens/DashboardScreen";
import { CaptureScreen } from "../screens/CaptureScreen";
import { ProfileScreen } from "../screens/ProfileScreen";

import { useAuthStore } from "../store/authStore";
import { useTheme } from "../theme/ThemeProvider";

export type AuthStackParamList = {
  Login: undefined;
  Register: undefined;
  ForgotPassword: undefined;
  ResetPassword: undefined;
};

type AppTabParamList = {
  Dashboard: undefined;
  Capture: undefined;
  Profile: undefined;
};

const AuthStack =
  createNativeStackNavigator<AuthStackParamList>();

const Tabs =
  createBottomTabNavigator<AppTabParamList>();

function AuthNavigator() {
  const { colors } = useTheme();

  return (
    <AuthStack.Navigator
      initialRouteName="Login"
      screenOptions={{
        headerShadowVisible: false,
        headerStyle: {
          backgroundColor: colors.background,
        },
        headerTintColor: colors.textPrimary,
        contentStyle: {
          backgroundColor: colors.background,
        },
      }}
    >
      <AuthStack.Screen
        component={LoginScreen}
        name="Login"
        options={{
          headerShown: false,
        }}
      />

      <AuthStack.Screen
        component={RegisterScreen}
        name="Register"
        options={{
          title: "Regisztráció",
        }}
      />

      <AuthStack.Screen
        component={ForgotPasswordScreen}
        name="ForgotPassword"
        options={{
          title: "Jelszó-helyreállítás",
        }}
      />

      <AuthStack.Screen
        component={ResetPasswordScreen}
        name="ResetPassword"
        options={{
          title: "Új jelszó",
        }}
      />
    </AuthStack.Navigator>
  );
}

function AppTabs() {
  const { colors } = useTheme();

  return (
    <Tabs.Navigator
      initialRouteName="Dashboard"
      screenOptions={{
        headerShadowVisible: false,
        headerStyle: {
          backgroundColor: colors.background,
        },
        headerTintColor: colors.textPrimary,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textSecondary,
        tabBarStyle: {
          backgroundColor: colors.surface,
          borderTopColor: colors.border,
        },
      }}
    >
      <Tabs.Screen
        component={DashboardScreen}
        name="Dashboard"
        options={{
          headerShown: false,
          title: "Áttekintés",
          tabBarLabel: "Áttekintés",
        }}
      />

      <Tabs.Screen
        component={CaptureScreen}
        name="Capture"
        options={{
          headerShown: false,
          title: "Rögzítés",
          tabBarLabel: "Rögzítés",
        }}
      />

      <Tabs.Screen
        component={ProfileScreen}
        name="Profile"
        options={{
          headerShown: false,
          title: "Profil",
          tabBarLabel: "Profil",
        }}
      />
    </Tabs.Navigator>
  );
}

function LoadingScreen() {
  const { colors } = useTheme();

  return (
    <View
      style={[
        styles.loadingContainer,
        {
          backgroundColor: colors.background,
        },
      ]}
    >
      <ActivityIndicator
        color={colors.primary}
        size="large"
      />
    </View>
  );
}

export function AppNavigator() {
  const { colors, mode } = useTheme();

  const isAuthenticated = useAuthStore(
    (state) => state.isAuthenticated
  );

  const isLoading = useAuthStore(
    (state) => state.isLoading
  );

  const restoreSession = useAuthStore(
    (state) => state.restoreSession
  );

  const navigationTheme = useMemo<Theme>(() => {
    const baseTheme =
      mode === "dark" ? DarkTheme : DefaultTheme;

    return {
      ...baseTheme,
      dark: mode === "dark",
      colors: {
        ...baseTheme.colors,
        primary: colors.primary,
        background: colors.background,
        card: colors.surface,
        text: colors.textPrimary,
        border: colors.border,
        notification: colors.danger,
      },
    };
  }, [colors, mode]);

  useEffect(() => {
    void restoreSession();
  }, [restoreSession]);

  if (isLoading) {
    return <LoadingScreen />;
  }

  return (
    <NavigationContainer theme={navigationTheme}>
      {isAuthenticated ? (
        <AppTabs />
      ) : (
        <AuthNavigator />
      )}
    </NavigationContainer>
  );
}

const styles = StyleSheet.create({
  loadingContainer: {
    alignItems: "center",
    flex: 1,
    justifyContent: "center",
  },
});