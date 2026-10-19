import { useState } from "react";
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";

import { ShiftForm } from "../components/capture/ShiftForm";
import { FuelFillUpForm } from "../components/capture/FuelFillUpForm";

type CaptureType = "shift" | "fuel";

export function CaptureScreen() {
  const [captureType, setCaptureType] =
    useState<CaptureType>("shift");

  const [fuelOpened, setFuelOpened] = useState(false);

  function selectType(type: CaptureType) {
    if (type === "fuel") {
      setFuelOpened(true);
    }

    setCaptureType(type);
  }

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === "ios" ? "padding" : undefined}
      style={styles.screen}
    >
      <ScrollView
        contentContainerStyle={styles.container}
        keyboardShouldPersistTaps="handled"
      >
        <View style={styles.header}>
          <Text style={styles.title}>Adatok rögzítése</Text>

          <Text style={styles.subtitle}>
            Válaszd ki, mit szeretnél rögzíteni.
          </Text>
        </View>

        <View style={styles.typeSelector}>
          <Pressable
            accessibilityRole="tab"
            accessibilityState={{
              selected: captureType === "shift",
            }}
            onPress={() => selectType("shift")}
            style={[
              styles.typeButton,
              captureType === "shift"
                ? styles.activeTypeButton
                : null,
            ]}
          >
            <Text
              style={[
                styles.typeButtonText,
                captureType === "shift"
                  ? styles.activeTypeButtonText
                  : null,
              ]}
            >
              Műszak
            </Text>
          </Pressable>

          <Pressable
            accessibilityRole="tab"
            accessibilityState={{
              selected: captureType === "fuel",
            }}
            onPress={() => selectType("fuel")}
            style={[
              styles.typeButton,
              captureType === "fuel"
                ? styles.activeTypeButton
                : null,
            ]}
          >
            <Text
              style={[
                styles.typeButtonText,
                captureType === "fuel"
                  ? styles.activeTypeButtonText
                  : null,
              ]}
            >
              Tankolás
            </Text>
          </Pressable>
        </View>

        <Text style={styles.sectionTitle}>
          {captureType === "shift"
            ? "Műszak rögzítése"
            : "Tankolás rögzítése"}
        </Text>

        <View
          style={
            captureType !== "shift" ? styles.hidden : undefined
          }
        >
          <ShiftForm />
        </View>

        {fuelOpened ? (
          <View
            style={
              captureType !== "fuel" ? styles.hidden : undefined
            }
          >
            <FuelFillUpForm />
          </View>
        ) : null}
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  screen: {
    backgroundColor: "#F6F7FB",
    flex: 1,
  },
  container: {
    alignSelf: "center",
    flexGrow: 1,
    maxWidth: 680,
    padding: 24,
    paddingBottom: 40,
    width: "100%",
  },
  header: {
    marginBottom: 22,
  },
  title: {
    color: "#1B1D28",
    fontSize: 26,
    fontWeight: "700",
  },
  subtitle: {
    color: "#666B7A",
    fontSize: 16,
    lineHeight: 23,
    marginTop: 10,
  },
  typeSelector: {
    backgroundColor: "#E7E9F0",
    borderRadius: 12,
    flexDirection: "row",
    gap: 6,
    marginBottom: 24,
    padding: 5,
  },
  typeButton: {
    alignItems: "center",
    borderRadius: 9,
    flex: 1,
    justifyContent: "center",
    minHeight: 44,
    paddingHorizontal: 12,
  },
  activeTypeButton: {
    backgroundColor: "#365CF5",
  },
  typeButtonText: {
    color: "#666B7A",
    fontSize: 15,
    fontWeight: "600",
  },
  activeTypeButtonText: {
    color: "#FFFFFF",
  },
  sectionTitle: {
    color: "#1B1D28",
    fontSize: 20,
    fontWeight: "700",
    marginBottom: 20,
  },
  hidden: {
    display: "none",
  },
});