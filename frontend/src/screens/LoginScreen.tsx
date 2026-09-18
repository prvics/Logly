import { useState } from "react";
import {
  ActivityIndicator,
  Alert,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import type { NativeStackScreenProps } from "@react-navigation/native-stack";
import { login } from "../api/endpoints";
import { ApiError } from "../api/client";
import { useAuthStore } from "../store/authStore";
import type { AuthStackParamList } from "../navigation/AppNavigator";

type Props = NativeStackScreenProps<AuthStackParamList, "Login">;

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function LoginScreen({ navigation }: Props) {
  const loginWithResponse = useAuthStore(
    (state) => state.loginWithResponse
  );

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [emailError, setEmailError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function validateForm(): boolean {
    let isValid = true;

    const trimmedEmail = email.trim();

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

async function handleLogin() {
  console.log("LOGIN BUTTON PRESSED");

  if (isSubmitting) {
    console.log("LOGIN STOPPED: already submitting");
    return;
  }

  const isValid = validateForm();

  if (!isValid) {
    console.log("LOGIN STOPPED: form validation failed", {
      email: email.trim(),
      hasPassword: Boolean(password),
    });
    return;
  }

  console.log("LOGIN REQUEST STARTING", {
    email: email.trim(),
  });

  setIsSubmitting(true);

  try {
    const response = await login({
      email: email.trim(),
      password,
    });

    console.log("LOGIN RESPONSE RECEIVED", {
      hasAccessToken: Boolean(response.accessToken),
      user: response.user,
    });

    await loginWithResponse(response);

    console.log("LOGIN SUCCESS: auth state updated");
  } catch (error) {
    console.error("LOGIN REQUEST FAILED", error);

    if (error instanceof ApiError) {
      const apiEmailError = error.fieldError("Email");
      const apiPasswordError = error.fieldError("Password");

      setEmailError(apiEmailError ?? null);
      setPasswordError(apiPasswordError ?? null);

      if (!apiEmailError && !apiPasswordError) {
        Alert.alert("Bejelentkezés sikertelen", error.message);
      }
    } else {
      Alert.alert(
        "Bejelentkezés sikertelen",
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
      <View style={styles.content}>
        <Text style={styles.title}>Logly</Text>
        <Text style={styles.subtitle}>
          Kövesd a futár bevételeidet és kiadásaidat.
        </Text>

        <View style={styles.form}>
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
            autoComplete="password"
            onChangeText={(value) => {
              setPassword(value);
              if (passwordError) {
                setPasswordError(null);
              }
            }}
            placeholder="Jelszavad"
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
            onPress={handleLogin}
            style={({ pressed }) => [
              styles.primaryButton,
              pressed && !isSubmitting ? styles.buttonPressed : null,
              isSubmitting ? styles.buttonDisabled : null,
            ]}
          >
            {isSubmitting ? (
              <ActivityIndicator color="#FFFFFF" />
            ) : (
              <Text style={styles.primaryButtonText}>Bejelentkezés</Text>
            )}
          </Pressable>

          <Pressable
            accessibilityRole="button"
            disabled={isSubmitting}
            onPress={() => navigation.navigate("Register")}
            style={styles.secondaryButton}
          >
            <Text style={styles.secondaryButtonText}>
              Még nincs fiókod? Regisztráció
            </Text>
          </Pressable>
        </View>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#F6F7FB",
    flex: 1,
  },
  content: {
    flex: 1,
    justifyContent: "center",
    padding: 24,
  },
  title: {
    color: "#1B1D28",
    fontSize: 36,
    fontWeight: "700",
    letterSpacing: -1,
    textAlign: "center",
  },
  subtitle: {
    color: "#666B7A",
    fontSize: 16,
    lineHeight: 23,
    marginBottom: 36,
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
    marginTop: 6,
    minHeight: 44,
    justifyContent: "center",
  },
  secondaryButtonText: {
    color: "#365CF5",
    fontSize: 14,
    fontWeight: "600",
  },
});