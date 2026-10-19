import { useEffect, useRef, useState } from "react";
import {
  ActivityIndicator,
  Animated,
  Easing,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import * as Haptics from "expo-haptics";
import { LoglyLogo } from "../components/ui/LoglyLogo";
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

  const submittingRef = useRef(false);

  const shakeOffset = useRef(new Animated.Value(0)).current;
  const errorProgress = useRef(new Animated.Value(0)).current;

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [emailError, setEmailError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);

  const [isSubmitting, setIsSubmitting] = useState(false);

  const buttonBackgroundColor = errorProgress.interpolate({
    inputRange: [0, 1],
    outputRange: ["#365CF5", "#D92D20"],
    extrapolate: "clamp",
  });

  useEffect(() => {
    return () => {
      shakeOffset.stopAnimation();
      errorProgress.stopAnimation();
    };
  }, [shakeOffset, errorProgress]);

  function shakeLoginButton() {
    shakeOffset.stopAnimation();
    shakeOffset.setValue(0);

    errorProgress.stopAnimation();

    Animated.parallel([
      Animated.sequence(
        [-10, 10, -8, 8, -4, 4, 0].map((toValue) =>
          Animated.timing(shakeOffset, {
            toValue,
            duration: 50,
            useNativeDriver: Platform.OS !== "web",
          })
        )
      ),

      Animated.sequence([
        Animated.timing(errorProgress, {
          toValue: 1,
          duration: 150,
          easing: Easing.inOut(Easing.quad),
          useNativeDriver: false,
        }),

        Animated.delay(150),

        Animated.timing(errorProgress, {
          toValue: 0,
          duration: 300,
          easing: Easing.inOut(Easing.quad),
          useNativeDriver: false,
        }),
      ]),
    ]).start();
  }

  function validateForm(): boolean {
    let isValid = true;

    const trimmedEmail = email.trim();

    if (!trimmedEmail) {
      setEmailError("Az e-mail-cím megadása kötelező.");
      isValid = false;
    } else if (!EMAIL_PATTERN.test(trimmedEmail)) {
      setEmailError("Adj meg érvényes e-mail-címet.");
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
    if (submittingRef.current) {
      return;
    }

    setFormError(null);

    if (!validateForm()) {
      shakeLoginButton();
      return;
    }

    submittingRef.current = true;
    setIsSubmitting(true);

    try {
      const response = await login({
        email: email.trim(),
        password,
      });

      if (
        typeof response.accessToken !== "string" ||
        !response.accessToken.trim() ||
        typeof response.user !== "object" ||
        response.user === null
      ) {
        throw new Error("Invalid authentication response.");
      }

      await loginWithResponse(response);
    } catch (error) {
      shakeLoginButton();

      if (error instanceof ApiError) {
        if (error.status === 401) {
          setFormError("Hibás e-mail-cím vagy jelszó.");
          return;
        }

        const apiEmailError = error.fieldError("Email");
        const apiPasswordError = error.fieldError("Password");

        setEmailError(apiEmailError);
        setPasswordError(apiPasswordError);

        if (!apiEmailError && !apiPasswordError) {
          setFormError(error.message);
        }
      } else {
        setFormError(
          "Nem sikerült bejelentkezni. Próbáld újra később."
        );
      }
    } finally {
      submittingRef.current = false;
      setIsSubmitting(false);
    }
  }

  function handleLoginPress() {
    if (submittingRef.current) {
      return;
    }

    void Haptics.impactAsync(
      Haptics.ImpactFeedbackStyle.Light
    ).catch(() => {
    });

    void handleLogin();
  }

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === "ios" ? "padding" : undefined}
      style={styles.container}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <View style={styles.content}>
          <LoglyLogo
            width={240}
            style={styles.logo}
          />

          <Text style={styles.subtitle}>
            Kövesd a futár bevételeidet és kiadásaidat.
          </Text>

          <View style={styles.form}>
            <Text style={styles.label}>E-mail-cím</Text>

            <TextInput
              accessibilityLabel="E-mail-cím"
              autoCapitalize="none"
              autoComplete="email"
              autoCorrect={false}
              editable={!isSubmitting}
              keyboardType="email-address"
              onChangeText={(value) => {
                setEmail(value);
                setEmailError(null);
                setFormError(null);
              }}
              placeholder="pelda@email.com"
              placeholderTextColor="#8A8A8A"
              style={[
                styles.input,
                emailError ? styles.inputError : null,
              ]}
              value={email}
            />

            {emailError ? (
              <Text style={styles.errorText}>{emailError}</Text>
            ) : null}

            <Text style={styles.label}>Jelszó</Text>

            <TextInput
              accessibilityLabel="Jelszó"
              autoCapitalize="none"
              autoComplete="password"
              autoCorrect={false}
              editable={!isSubmitting}
              onChangeText={(value) => {
                setPassword(value);
                setPasswordError(null);
                setFormError(null);
              }}
              onSubmitEditing={() => void handleLogin()}
              placeholder="Jelszavad"
              placeholderTextColor="#8A8A8A"
              returnKeyType="go"
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

            {formError ? (
              <View
                accessibilityRole="alert"
                accessibilityLiveRegion="polite"
                style={styles.errorBox}
              >
                <Text style={styles.errorBoxTitle}>
                  Bejelentkezés sikertelen
                </Text>

                <Text style={styles.errorBoxText}>{formError}</Text>
              </View>
            ) : null}

            <Animated.View
              style={{
                transform: [{ translateX: shakeOffset }],
              }}
            >
              <Animated.View
                style={[
                  styles.primaryButton,
                  {
                    backgroundColor: buttonBackgroundColor,
                    overflow: "hidden",
                  },
                  isSubmitting ? styles.buttonDisabled : null,
                ]}
              >
                <Pressable
                  accessibilityRole="button"
                  accessibilityState={{
                    disabled: isSubmitting,
                    busy: isSubmitting,
                  }}
                  disabled={isSubmitting}
                  onPress={handleLoginPress}
                  style={({ pressed }) => [
                    styles.primaryButtonContent,
                    pressed && !isSubmitting
                      ? styles.buttonPressed
                      : null,
                  ]}
                >
                  {isSubmitting ? (
                    <ActivityIndicator color="#FFFFFF" />
                  ) : (
                    <Text style={styles.primaryButtonText}>
                      Bejelentkezés
                    </Text>
                  )}
                </Pressable>
              </Animated.View>
            </Animated.View>

            <Pressable
              accessibilityRole="button"
              accessibilityState={{ disabled: isSubmitting }}
              disabled={isSubmitting}
              onPress={() => navigation.navigate("ForgotPassword")}
              style={styles.secondaryButton}
            >
              <Text style={styles.secondaryButtonText}>
                Elfelejtetted a jelszavad?
              </Text>
            </Pressable>

            <Pressable
              accessibilityRole="button"
              accessibilityState={{ disabled: isSubmitting }}
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
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: "#F6F7FB",
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    justifyContent: "center",
    padding: 24,
  },
  content: {
    alignSelf: "center",
    maxWidth: 440,
    width: "100%",
  },
  logo: {
    alignSelf: "center",
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
  errorBox: {
    backgroundColor: "#FFF4F2",
    borderColor: "#F5C2BC",
    borderRadius: 12,
    borderWidth: 1,
    marginTop: 8,
    padding: 14,
  },
  errorBoxTitle: {
    color: "#9E251B",
    fontSize: 14,
    fontWeight: "700",
  },
  errorBoxText: {
    color: "#9E251B",
    fontSize: 14,
    lineHeight: 20,
    marginTop: 4,
  },
  primaryButton: {
    alignItems: "center",
    backgroundColor: "#365CF5",
    borderRadius: 12,
    justifyContent: "center",
    marginTop: 18,
    minHeight: 52,
  },
  primaryButtonContent: {
    alignItems: "center",
    justifyContent: "center",
    minHeight: 52,
    paddingHorizontal: 16,
    width: "100%",
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