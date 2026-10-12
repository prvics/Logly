import { useEffect } from "react";
import { ActivityIndicator, StyleSheet, View } from "react-native";
import { NavigationContainer } from "@react-navigation/native";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";

import { LoginScreen } from "../screens/LoginScreen";
import { RegisterScreen } from "../screens/RegisterScreen";
import { DashboardScreen } from "../screens/DashboardScreen";
import { CaptureScreen } from "../screens/CaptureScreen";
import { ProfileScreen } from "../screens/ProfileScreen";
import { useAuthStore } from "../store/authStore";

export type AuthStackParamList = {
  Login: undefined;
  Register: undefined;
};

type AppTabParamList = {
  Dashboard: undefined;
  Capture: undefined;
  Profile: undefined;
};

const AuthStack = createNativeStackNavigator<AuthStackParamList>();
const Tabs = createBottomTabNavigator<AppTabParamList>();

function AuthNavigator() {
  return (
    <AuthStack.Navigator
      initialRouteName="Login"
      screenOptions={{
        headerShadowVisible: false,
        headerStyle: { backgroundColor: "#F6F7FB" },
        headerTintColor: "#1B1D28",
      }}
    >
      <AuthStack.Screen
        component={LoginScreen}
        name="Login"
        options={{ headerShown: false }}
      />
      <AuthStack.Screen
        component={RegisterScreen}
        name="Register"
        options={{ title: "Regisztráció" }}
      />
    </AuthStack.Navigator>
  );
}

function AppTabs() {
  return (
    <Tabs.Navigator
      initialRouteName="Dashboard"
      screenOptions={{
        headerShadowVisible: false,
        headerStyle: { backgroundColor: "#F6F7FB" },
        headerTintColor: "#1B1D28",
        tabBarActiveTintColor: "#365CF5",
        tabBarInactiveTintColor: "#777B88",
        tabBarStyle: {
          backgroundColor: "#FFFFFF",
          borderTopColor: "#E7E9F0",
        },
      }}
    >
      <Tabs.Screen
        component={DashboardScreen}
        name="Dashboard"
        options={{
          title: "Logly",
          tabBarLabel: "Áttekintés",
        }}
      />
      <Tabs.Screen
        component={CaptureScreen}
        name="Capture"
        options={{
          title: "Rögzítés",
          tabBarLabel: "Rögzítés",
        }}
      />
      <Tabs.Screen
        component={ProfileScreen}
        name="Profile"
        options={{
          title: "Profil",
          tabBarLabel: "Profil",
        }}
      />
    </Tabs.Navigator>
  );
}

function LoadingScreen() {
  return (
    <View style={styles.loadingContainer}>
      <ActivityIndicator color="#365CF5" size="large" />
    </View>
  );
}

export function AppNavigator() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const isLoading = useAuthStore((state) => state.isLoading);
  const restoreSession = useAuthStore((state) => state.restoreSession);

  useEffect(() => {
    void restoreSession();
  }, [restoreSession]);

  if (isLoading) {
    return <LoadingScreen />;
  }

  return (
    <NavigationContainer>
      {isAuthenticated ? <AppTabs /> : <AuthNavigator />}
    </NavigationContainer>
  );
}

const styles = StyleSheet.create({
  loadingContainer: {
    alignItems: "center",
    backgroundColor: "#F6F7FB",
    flex: 1,
    justifyContent: "center",
  },
});