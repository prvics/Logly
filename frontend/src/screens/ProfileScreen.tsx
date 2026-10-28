import {
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";

import { UserCompaniesSection } from "../components/profile/UserCompaniesSection";
import { UserVehiclesSection } from "../components/profile/UserVehiclesSection";
import { ThemeSwitch } from "../components/ui/ThemeSwitch";
import { useAuthStore } from "../store/authStore";
import { useTheme } from "../theme/ThemeProvider";

export function ProfileScreen() {
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  const { colors, mode, setTheme } = useTheme();

  return (
    <ScrollView
      style={[
        styles.screen,
        { backgroundColor: colors.background },
      ]}
      contentContainerStyle={styles.container}
      keyboardShouldPersistTaps="handled"
    >
      <View style={styles.content}>
        <Text
          style={[
            styles.title,
            { color: colors.textPrimary },
          ]}
        >
          Profil
        </Text>

        <Text
          style={[
            styles.label,
            { color: colors.textSecondary },
          ]}
        >
          Név
        </Text>

        <Text
          style={[
            styles.value,
            { color: colors.textPrimary },
          ]}
        >
          {user?.displayName || "Nincs megadva"}
        </Text>

        <Text
          style={[
            styles.label,
            { color: colors.textSecondary },
          ]}
        >
          E-mail
        </Text>

        <Text
          style={[
            styles.value,
            { color: colors.textPrimary },
          ]}
        >
          {user?.email || "Nincs megadva"}
        </Text>

        <UserCompaniesSection />

        <UserVehiclesSection />

        <View
          style={[
            styles.appearanceCard,
            {
              backgroundColor: colors.surface,
              borderColor: colors.border,
            },
          ]}
        >
          <Text
            style={[
              styles.sectionTitle,
              { color: colors.textPrimary },
            ]}
          >
            Megjelenés
          </Text>

          <View style={styles.themeRow}>
            <View style={styles.themeText}>
              <Text
                style={[
                  styles.themeTitle,
                  { color: colors.textPrimary },
                ]}
              >
                Sötét mód
              </Text>

              <Text
                style={[
                  styles.themeSubtitle,
                  { color: colors.textSecondary },
                ]}
              >
                Váltás a világos és sötét megjelenés között.
              </Text>
            </View>

            <ThemeSwitch
              value={mode === "dark"}
              onValueChange={(enabled) =>
                setTheme(enabled ? "dark" : "light")
              }
              accessibilityLabel="Sötét mód bekapcsolása vagy kikapcsolása"
            />
          </View>
        </View>

        <Pressable
          accessibilityRole="button"
          onPress={() => void logout()}
          style={({ pressed }) => [
            styles.logoutButton,
            { borderColor: colors.danger },
            pressed ? styles.pressed : null,
          ]}
        >
          <Text
            style={[
              styles.logoutText,
              { color: colors.danger },
            ]}
          >
            Kijelentkezés
          </Text>
        </Pressable>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
  },
  container: {
    flexGrow: 1,
    padding: 24,
    paddingBottom: 40,
  },
  content: {
    alignSelf: "center",
    maxWidth: 720,
    width: "100%",
  },
  title: {
    fontSize: 26,
    fontWeight: "700",
    marginBottom: 10,
  },
  label: {
    fontSize: 14,
    marginTop: 14,
  },
  value: {
    fontSize: 16,
    fontWeight: "600",
    marginTop: 4,
  },
  appearanceCard: {
    borderRadius: 16,
    borderWidth: 1,
    marginTop: 32,
    padding: 16,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: "700",
    marginBottom: 16,
  },
  themeRow: {
    alignItems: "center",
    flexDirection: "row",
    gap: 16,
  },
  themeText: {
    flex: 1,
  },
  themeTitle: {
    fontSize: 16,
    fontWeight: "600",
  },
  themeSubtitle: {
    fontSize: 14,
    lineHeight: 20,
    marginTop: 4,
  },
  logoutButton: {
    alignItems: "center",
    borderRadius: 12,
    borderWidth: 1,
    justifyContent: "center",
    marginTop: 40,
    minHeight: 50,
  },
  logoutText: {
    fontSize: 15,
    fontWeight: "700",
  },
  pressed: {
    opacity: 0.75,
  },
});