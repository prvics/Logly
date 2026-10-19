import { useRef, useState } from "react";
import type { ReactNode } from "react";
import {
  ActivityIndicator,
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
import {
  forgotPassword,
  resetPassword,
} from "../api/passwordReset";
import type { AuthStackParamList } from "../navigation/AppNavigator";

type ForgotProps = NativeStackScreenProps<
  AuthStackParamList,
  "ForgotPassword"
>;

type ResetProps = NativeStackScreenProps<
  AuthStackParamList,
  "ResetPassword"
>;

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const TOKEN_PATTERN = /^[a-fA-F0-9]{64}$/;

function Page({ children }: { children: ReactNode }) {
  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === "ios" ? "padding" : undefined}
      style={styles.container}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <View style={styles.content}>{children}</View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

function Message({
  children,
  error = false,
}: {
  children: string;
  error?: boolean;
}) {
  return (
    <View
      accessibilityLiveRegion="polite"
      style={[
        styles.messageBox,
        error ? styles.errorBox : styles.successBox,
      ]}
    >
      <Text
        style={[
          styles.messageText,
          error ? styles.errorText : styles.successText,
        ]}
      >
        {children}
      </Text>
    </View>
  );
}

function Button({
  title,
  onPress,
  loading = false,
  secondary = false,
  disabled = false,
}: {
  title: string;
  onPress: () => void;
  loading?: boolean;
  secondary?: boolean;
  disabled?: boolean;
}) {
  const isDisabled = disabled || loading;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={{
        disabled: isDisabled,
        busy: loading,
      }}
      disabled={isDisabled}
      onPress={onPress}
      style={({ pressed }) => [
        styles.button,
        secondary ? styles.secondaryButton : styles.primaryButton,
        pressed && !isDisabled ? styles.pressed : null,
        isDisabled ? styles.disabled : null,
      ]}
    >
      {loading ? (
        <ActivityIndicator color="#FFFFFF" />
      ) : (
        <Text
          style={
            secondary
              ? styles.secondaryButtonText
              : styles.primaryButtonText
          }
        >
          {title}
        </Text>
      )}
    </Pressable>
  );
}

export function ForgotPasswordScreen({
  navigation,
}: ForgotProps) {
  const submittingRef = useRef(false);

  const [email, setEmail] = useState("");
  const [emailError, setEmailError] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] =
    useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit() {
    if (submittingRef.current) {
      return;
    }

    setFormError(null);
    setSuccessMessage(null);

    const normalizedEmail = email.trim();

    if (!normalizedEmail) {
      setEmailError("Az e-mail-cím megadása kötelező.");
      return;
    }

    if (!EMAIL_PATTERN.test(normalizedEmail)) {
      setEmailError("Adj meg érvényes e-mail-címet.");
      return;
    }

    setEmailError(null);
    submittingRef.current = true;
    setIsSubmitting(true);

    try {
      const response = await forgotPassword({
        email: normalizedEmail,
      });

      setSuccessMessage(response.message);
    } catch (error) {
      if (error instanceof ApiError) {
        const fieldError = error.fieldError("Email");

        setEmailError(fieldError);
        setFormError(fieldError ? null : error.message);
      } else {
        setFormError(
          "Nem sikerült elküldeni a kérést. Próbáld újra később."
        );
      }
    } finally {
      submittingRef.current = false;
      setIsSubmitting(false);
    }
  }

  return (
    <Page>
      <Text style={styles.title}>Elfelejtett jelszó</Text>

      <Text style={styles.subtitle}>
        Add meg a fiókodhoz tartozó e-mail-címet, és kérj
        helyreállítási útmutatót.
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
            setSuccessMessage(null);
          }}
          onSubmitEditing={() => void handleSubmit()}
          placeholder="pelda@email.com"
          placeholderTextColor="#8A8A8A"
          returnKeyType="send"
          style={[
            styles.input,
            emailError ? styles.inputError : null,
          ]}
          value={email}
        />

        {emailError ? (
          <Text style={styles.fieldError}>{emailError}</Text>
        ) : null}

        {formError ? <Message error>{formError}</Message> : null}

        {successMessage ? (
          <Message>{successMessage}</Message>
        ) : null}

        <Button
          title="Helyreállítás kérése"
          loading={isSubmitting}
          onPress={() => void handleSubmit()}
        />

        <Button
          title="Megvan a helyreállító tokenem"
          secondary
          disabled={isSubmitting}
          onPress={() => navigation.navigate("ResetPassword")}
        />

        <Button
          title="Vissza a bejelentkezéshez"
          secondary
          disabled={isSubmitting}
          onPress={() =>
            navigation.reset({
              index: 0,
              routes: [{ name: "Login" }],
            })
          }
        />
      </View>
    </Page>
  );
}

export function ResetPasswordScreen({
  navigation,
}: ResetProps) {
  const submittingRef = useRef(false);

  const [token, setToken] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [tokenError, setTokenError] = useState<string | null>(null);
  const [passwordError, setPasswordError] =
    useState<string | null>(null);
  const [confirmError, setConfirmError] =
    useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] =
    useState<string | null>(null);

  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit() {
    if (submittingRef.current || successMessage) {
      return;
    }

    setFormError(null);

    const normalizedToken = token.trim();

    const nextTokenError = TOKEN_PATTERN.test(normalizedToken)
      ? null
      : "Másold be a levélből a teljes, 64 karakteres tokent.";

    const nextPasswordError =
      password.trim().length === 0
        ? "Az új jelszó megadása kötelező."
        : password.length < 12
          ? "Az új jelszónak legalább 12 karakterből kell állnia."
          : null;

    const nextConfirmError =
      confirmPassword !== password
        ? "A két jelszó nem egyezik."
        : null;

    setTokenError(nextTokenError);
    setPasswordError(nextPasswordError);
    setConfirmError(nextConfirmError);

    if (
      nextTokenError ||
      nextPasswordError ||
      nextConfirmError
    ) {
      return;
    }

    submittingRef.current = true;
    setIsSubmitting(true);

    try {
      const response = await resetPassword({
        token: normalizedToken,
        newPassword: password,
      });

      setToken("");
      setPassword("");
      setConfirmPassword("");
      setSuccessMessage(response.message);
    } catch (error) {
      if (error instanceof ApiError) {
        const apiTokenError = error.fieldError("Token");
        const apiPasswordError = error.fieldError("NewPassword");

        setTokenError(apiTokenError);
        setPasswordError(apiPasswordError);

        if (!apiTokenError && !apiPasswordError) {
          setFormError(error.message);
        }
      } else {
        setFormError(
          "Nem sikerült módosítani a jelszót. Próbáld újra később."
        );
      }
    } finally {
      submittingRef.current = false;
      setIsSubmitting(false);
    }
  }

  return (
    <Page>
      <Text style={styles.title}>Új jelszó</Text>

      <Text style={styles.subtitle}>
        Másold be az e-mailben kapott tokent, és adj meg
        egy legalább 12 karakteres új jelszót.
      </Text>

      <View style={styles.form}>
        {successMessage ? (
          <>
            <Message>{successMessage}</Message>

            <Button
              title="Vissza a bejelentkezéshez"
              onPress={() =>
                navigation.reset({
                  index: 0,
                  routes: [{ name: "Login" }],
                })
              }
            />
          </>
        ) : (
          <>
            <Text style={styles.label}>Helyreállító token</Text>

            <TextInput
              accessibilityLabel="Helyreállító token"
              autoCapitalize="none"
              autoCorrect={false}
              editable={!isSubmitting}
              onChangeText={(value) => {
                setToken(value);
                setTokenError(null);
                setFormError(null);
              }}
              placeholder="A levélben kapott token"
              placeholderTextColor="#8A8A8A"
              style={[
                styles.input,
                tokenError ? styles.inputError : null,
              ]}
              value={token}
            />

            {tokenError ? (
              <Text style={styles.fieldError}>{tokenError}</Text>
            ) : null}

            <Text style={styles.label}>Új jelszó</Text>

            <TextInput
              accessibilityLabel="Új jelszó"
              autoCapitalize="none"
              autoComplete="new-password"
              autoCorrect={false}
              editable={!isSubmitting}
              onChangeText={(value) => {
                setPassword(value);
                setPasswordError(null);
                setConfirmError(null);
                setFormError(null);
              }}
              placeholder="Legalább 12 karakter"
              placeholderTextColor="#8A8A8A"
              secureTextEntry
              style={[
                styles.input,
                passwordError ? styles.inputError : null,
              ]}
              value={password}
            />

            {passwordError ? (
              <Text style={styles.fieldError}>
                {passwordError}
              </Text>
            ) : null}

            <Text style={styles.label}>Új jelszó megerősítése</Text>

            <TextInput
              accessibilityLabel="Új jelszó megerősítése"
              autoCapitalize="none"
              autoComplete="new-password"
              autoCorrect={false}
              editable={!isSubmitting}
              onChangeText={(value) => {
                setConfirmPassword(value);
                setConfirmError(null);
                setFormError(null);
              }}
              onSubmitEditing={() => void handleSubmit()}
              placeholder="Új jelszó ismét"
              placeholderTextColor="#8A8A8A"
              returnKeyType="done"
              secureTextEntry
              style={[
                styles.input,
                confirmError ? styles.inputError : null,
              ]}
              value={confirmPassword}
            />

            {confirmError ? (
              <Text style={styles.fieldError}>{confirmError}</Text>
            ) : null}

            {formError ? (
              <Message error>{formError}</Message>
            ) : null}

            <Button
              title="Jelszó mentése"
              loading={isSubmitting}
              onPress={() => void handleSubmit()}
            />

            <Button
              title="Új helyreállítás kérése"
              secondary
              disabled={isSubmitting}
              onPress={() =>
                navigation.navigate("ForgotPassword")
              }
            />
          </>
        )}
      </View>
    </Page>
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
  title: {
    color: "#1B1D28",
    fontSize: 28,
    fontWeight: "700",
    textAlign: "center",
  },
  subtitle: {
    color: "#666B7A",
    fontSize: 16,
    lineHeight: 23,
    marginBottom: 28,
    marginTop: 12,
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
    paddingVertical: 12,
  },
  inputError: {
    borderColor: "#D92D20",
  },
  fieldError: {
    color: "#D92D20",
    fontSize: 13,
  },
  messageBox: {
    borderRadius: 12,
    borderWidth: 1,
    marginTop: 8,
    padding: 14,
  },
  errorBox: {
    backgroundColor: "#FFF4F2",
    borderColor: "#F5C2BC",
  },
  successBox: {
    backgroundColor: "#ECFDF3",
    borderColor: "#ABEFC6",
  },
  messageText: {
    fontSize: 14,
    lineHeight: 21,
  },
  errorText: {
    color: "#9E251B",
  },
  successText: {
    color: "#067647",
  },
  button: {
    alignItems: "center",
    borderRadius: 12,
    justifyContent: "center",
    minHeight: 52,
    paddingHorizontal: 12,
    paddingVertical: 12,
  },
  primaryButton: {
    backgroundColor: "#365CF5",
    marginTop: 16,
  },
  secondaryButton: {
    backgroundColor: "transparent",
  },
  primaryButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "700",
    textAlign: "center",
  },
  secondaryButtonText: {
    color: "#365CF5",
    fontSize: 14,
    fontWeight: "600",
    textAlign: "center",
  },
  pressed: {
    opacity: 0.85,
  },
  disabled: {
    opacity: 0.6,
  },
});