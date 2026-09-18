import { Pressable, StyleSheet, Text, View } from "react-native";
import { useAuthStore } from "../store/authStore";

export function DashboardScreen() {
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  const displayName = user?.displayName?.trim() || "Futár";

  async function handleLogout() {
    await logout();
  }

  return (
    <View style={styles.container}>
      <View>
        <Text style={styles.title}>Szia, {displayName}!</Text>
        <Text style={styles.subtitle}>
          A dashboard hamarosan itt lesz.
        </Text>
      </View>

      <Pressable
        accessibilityRole="button"
        onPress={handleLogout}
        style={({ pressed }) => [
          styles.logoutButton,
          pressed ? styles.buttonPressed : null,
        ]}
      >
        <Text style={styles.logoutButtonText}>Kijelentkezés</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#F6F7FB",
    flex: 1,
    justifyContent: "space-between",
    padding: 24,
    paddingVertical: 48,
  },
  title: {
    color: "#1B1D28",
    fontSize: 30,
    fontWeight: "700",
  },
  subtitle: {
    color: "#666B7A",
    fontSize: 16,
    marginTop: 8,
  },
  logoutButton: {
    alignItems: "center",
    borderColor: "#D92D20",
    borderRadius: 12,
    borderWidth: 1,
    justifyContent: "center",
    minHeight: 52,
  },
  buttonPressed: {
    opacity: 0.75,
  },
  logoutButtonText: {
    color: "#D92D20",
    fontSize: 16,
    fontWeight: "700",
  },
});