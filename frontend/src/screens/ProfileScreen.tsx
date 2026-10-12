import { Pressable, StyleSheet, Text, View } from "react-native";
import { useAuthStore } from "../store/authStore";

export function ProfileScreen() {
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Profil</Text>
      <Text style={styles.label}>Név</Text>
      <Text style={styles.value}>{user?.displayName || "Nincs megadva"}</Text>
      <Text style={styles.label}>E-mail</Text>
      <Text style={styles.value}>{user?.email || "Nincs megadva"}</Text>

      <Pressable
        accessibilityRole="button"
        onPress={() => void logout()}
        style={({ pressed }) => [
          styles.logoutButton,
          pressed ? styles.pressed : null,
        ]}
      >
        <Text style={styles.logoutText}>Kijelentkezés</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#F6F7FB",
    flex: 1,
    padding: 24,
  },
  title: {
    color: "#1B1D28",
    fontSize: 26,
    fontWeight: "700",
    marginBottom: 24,
  },
  label: {
    color: "#666B7A",
    fontSize: 14,
    marginTop: 14,
  },
  value: {
    color: "#1B1D28",
    fontSize: 16,
    fontWeight: "600",
    marginTop: 4,
  },
  logoutButton: {
    alignItems: "center",
    borderColor: "#D92D20",
    borderRadius: 12,
    borderWidth: 1,
    justifyContent: "center",
    marginTop: 40,
    minHeight: 50,
  },
  logoutText: {
    color: "#D92D20",
    fontSize: 15,
    fontWeight: "700",
  },
  pressed: {
    opacity: 0.75,
  },
});