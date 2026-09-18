import { useEffect } from "react";
import { ActivityIndicator, StyleSheet, View } from "react-native";
import { NavigationContainer } from "@react-navigation/native";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { LoginScreen } from "../screens/LoginScreen";
import { RegisterScreen } from "../screens/RegisterScreen";
import { DashboardScreen } from "../screens/DashboardScreen";
import { useAuthStore } from "../store/authStore";

export type AuthStackParamList = {
  Login: undefined;
  Register: undefined;
};

type AppStackParamList = {
  Dashboard: undefined;
};

const AuthStack = createNativeStackNavigator<AuthStackParamList>();
const AppStack = createNativeStackNavigator<AppStackParamList>();

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

function AuthenticatedNavigator() {
  return (
    <AppStack.Navigator
      screenOptions={{
        headerShadowVisible: false,
        headerStyle: { backgroundColor: "#F6F7FB" },
        headerTintColor: "#1B1D28",
      }}
    >
      <AppStack.Screen
        component={DashboardScreen}
        name="Dashboard"
        options={{ title: "Logly" }}
      />
    </AppStack.Navigator>
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
  const isAuthenticated = useAuthStore(
    (state) => state.isAuthenticated
  );
  const isLoading = useAuthStore((state) => state.isLoading);
  const restoreSession = useAuthStore(
    (state) => state.restoreSession
  );

  useEffect(() => {
    void restoreSession();
  }, [restoreSession]);

  if (isLoading) {
    return <LoadingScreen />;
  }

  return (
    <NavigationContainer>
      {isAuthenticated ? (
        <AuthenticatedNavigator />
      ) : (
        <AuthNavigator />
      )}
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