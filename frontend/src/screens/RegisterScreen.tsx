import { useState } from "react";
import {
  ActivityIndicator,
  Alert,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { ApiError } from "../api/client";
import { register } from "../api/endpoints";
import type { AuthStackParamList } from "../navigation/AppNavigator";
import { useAuthStore } from "../store/authStore";

type Props = NativeStackScreenProps<AuthStackParamList, "Register">;

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function RegisterScreen({ navigation }: Props) {
  const loginWithResponse = useAuthStore(
    (state) => state.loginWithResponse
  );

  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [displayNameError, setDisplayNameError] = useState<string | null>(
    null
  );
  const [emailError, setEmailError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function validateForm(): boolean {
    let isValid = true;

    const trimmedDisplayName = displayName.trim();
    const trimmedEmail = email.trim();

    if (!trimmedDisplayName) {
      setDisplayNameError("A megjelenített név megadása kötelező.");
      isValid = false;
    } else if (trimmedDisplayName.length > 100) {
      setDisplayNameError(
        "A megjelenített név legfeljebb 100 karakter lehet."
      );
      isValid = false;
    } else {
      setDisplayNameError(null);
    }

    if (!trimmedEmail) {
      setEmailError("Az e-mail cím megadása kötelező.");
      isValid = false;
    } else if (!EMAIL_PATTERN.test(trimmedEmail)) {
      setEmailError("Adj meg érvényes e-mail címet.");
      isValid = false;
    } else {
      setEmailError(null);
    }

    if (!password) {
      setPasswordError("A jelszó megadása kötelező.");
      isValid = false;
    } else {
      setPasswordError(null);
    }

    return isValid;
  }

  async function handleRegister() {
    if (isSubmitting || !validateForm()) {
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await register({
        displayName: displayName.trim(),
        email: email.trim(),
        password,
      });

      await loginWithResponse(response);
    } catch (error) {
      if (error instanceof ApiError) {
        setDisplayNameError(error.fieldError("DisplayName") ?? null);
        setEmailError(error.fieldError("Email") ?? null);
        setPasswordError(error.fieldError("Password") ?? null);

        if (
          !error.fieldError("DisplayName") &&
          !error.fieldError("Email") &&
          !error.fieldError("Password")
        ) {
          Alert.alert("Regisztráció sikertelen", error.message);
        }
      } else {
        Alert.alert(
          "Regisztráció sikertelen",
          "Váratlan hiba történt. Próbáld meg később."
        );
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === "ios" ? "padding" : undefined}
      style={styles.container}
    >
      <ScrollView
        contentContainerStyle={styles.content}
        keyboardShouldPersistTaps="handled"
      >
        <View>
          <Text style={styles.title}>Fiók létrehozása</Text>
          <Text style={styles.subtitle}>
            Kezdd el követni a futár bevételeidet és kiadásaidat.
          </Text>

          <View style={styles.form}>
            <Text style={styles.label}>Megjelenített név</Text>
            <TextInput
              autoCapitalize="words"
              autoComplete="name"
              maxLength={100}
              onChangeText={(value) => {
                setDisplayName(value);
                if (displayNameError) {
                  setDisplayNameError(null);
                }
              }}
              placeholder="Például: Péter"
              placeholderTextColor="#8A8A8A"
              style={[
                styles.input,
                displayNameError ? styles.inputError : null,
              ]}
              value={displayName}
            />
            {displayNameError ? (
              <Text style={styles.errorText}>{displayNameError}</Text>
            ) : null}

            <Text style={styles.label}>E-mail cím</Text>
            <TextInput
              autoCapitalize="none"
              autoComplete="email"
              autoCorrect={false}
              keyboardType="email-address"
              onChangeText={(value) => {
                setEmail(value);
                if (emailError) {
                  setEmailError(null);
                }
              }}
              placeholder="pelda@email.com"
              placeholderTextColor="#8A8A8A"
              style={[styles.input, emailError ? styles.inputError : null]}
              value={email}
            />
            {emailError ? (
              <Text style={styles.errorText}>{emailError}</Text>
            ) : null}

            <Text style={styles.label}>Jelszó</Text>
            <TextInput
              autoComplete="new-password"
              onChangeText={(value) => {
                setPassword(value);
                if (passwordError) {
                  setPasswordError(null);
                }
              }}
              placeholder="Válassz jelszót"
              placeholderTextColor="#8A8A8A"
              secureTextEntry
              style={[
                styles.input,
                passwordError ? styles.inputError : null,
              ]}
              value={password}
            />
            {passwordError ? (
              <Text style={styles.errorText}>{passwordError}</Text>
            ) : null}

            <Pressable
              accessibilityRole="button"
              disabled={isSubmitting}
              onPress={handleRegister}
              style={({ pressed }) => [
                styles.primaryButton,
                pressed && !isSubmitting ? styles.buttonPressed : null,
                isSubmitting ? styles.buttonDisabled : null,
              ]}
            >
              {isSubmitting ? (
                <ActivityIndicator color="#FFFFFF" />
              ) : (
                <Text style={styles.primaryButtonText}>
                  Regisztráció
                </Text>
              )}
            </Pressable>

            <Pressable
              accessibilityRole="button"
              disabled={isSubmitting}
              onPress={() => navigation.goBack()}
              style={styles.secondaryButton}
            >
              <Text style={styles.secondaryButtonText}>
                Már van fiókod? Bejelentkezés
              </Text>
            </Pressable>
          </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#F6F7FB",
    flex: 1,
  },
  content: {
    flexGrow: 1,
    justifyContent: "center",
    padding: 24,
  },
  title: {
    color: "#1B1D28",
    fontSize: 30,
    fontWeight: "700",
    letterSpacing: -0.5,
    textAlign: "center",
  },
  subtitle: {
    color: "#666B7A",
    fontSize: 16,
    lineHeight: 23,
    marginBottom: 28,
    marginTop: 10,
    textAlign: "center",
  },
  form: {
    gap: 10,
  },
  label: {
    color: "#2A2D3A",
    fontSize: 14,
    fontWeight: "600",
    marginTop: 6,
  },
  input: {
    backgroundColor: "#FFFFFF",
    borderColor: "#D9DCE5",
    borderRadius: 12,
    borderWidth: 1,
    color: "#1B1D28",
    fontSize: 16,
    minHeight: 52,
    paddingHorizontal: 16,
  },
  inputError: {
    borderColor: "#D92D20",
  },
  errorText: {
    color: "#D92D20",
    fontSize: 13,
    marginTop: -4,
  },
  primaryButton: {
    alignItems: "center",
    backgroundColor: "#365CF5",
    borderRadius: 12,
    justifyContent: "center",
    marginTop: 18,
    minHeight: 52,
  },
  buttonPressed: {
    opacity: 0.85,
  },
  buttonDisabled: {
    opacity: 0.6,
  },
  primaryButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "700",
  },
  secondaryButton: {
    alignItems: "center",
    justifyContent: "center",
    marginTop: 6,
    minHeight: 44,
  },
  secondaryButtonText: {
    color: "#365CF5",
    fontSize: 14,
    fontWeight: "600",
  },
});