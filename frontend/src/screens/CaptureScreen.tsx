import { StyleSheet, Text, View } from "react-native";

export function CaptureScreen() {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Adatok rögzítése</Text>
      <Text style={styles.subtitle}>
        Ide kerülnek a műszak, tankolás és kiadás felvételi űrlapjai.
      </Text>
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
  },
  subtitle: {
    color: "#666B7A",
    fontSize: 16,
    lineHeight: 23,
    marginTop: 10,
  },
});