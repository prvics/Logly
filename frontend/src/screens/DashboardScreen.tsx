import { useCallback, useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { useFocusEffect } from "@react-navigation/native";
import { getDashboardSummary } from "../api/endpoints";
import { ApiError } from "../api/client";
import type {
  DashboardSummaryResponse,
} from "../types/api";
import { useAuthStore } from "../store/authStore";

const formatHuf = (amount: number): string =>
  new Intl.NumberFormat("hu-HU", {
    style: "currency",
    currency: "HUF",
    maximumFractionDigits: 0,
  }).format(amount);

const formatNumber = (value: number, digits = 1): string =>
  new Intl.NumberFormat("hu-HU", {
    maximumFractionDigits: digits,
  }).format(value);

function getCurrentMonthRange() {
  const now = new Date();
  const from = new Date(now.getFullYear(), now.getMonth(), 1);
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0);

  const formatDate = (date: Date): string => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  return {
    from: formatDate(from),
    to: formatDate(to),
  };
}

function MetricCard({
  label,
  value,
  valueColor,
}: {
  label: string;
  value: string;
  valueColor?: string;
}) {
  return (
    <View style={styles.metricCard}>
      <Text style={styles.metricLabel}>{label}</Text>
      <Text
        style={[
          styles.metricValue,
          valueColor ? { color: valueColor } : null,
        ]}
      >
        {value}
      </Text>
    </View>
  );
}

export function DashboardScreen() {
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  const [summary, setSummary] = useState<DashboardSummaryResponse | null>(
    null
  );
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const displayName = user?.displayName?.trim() || "Futár";

  const loadSummary = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const data = await getDashboardSummary(getCurrentMonthRange());
      setSummary(data);
    } catch (error) {
      setSummary(null);

      if (error instanceof ApiError) {
        setErrorMessage(error.message);
      } else if (error instanceof Error) {
        setErrorMessage(error.message);
      } else {
        setErrorMessage("Nem sikerült betölteni a dashboard adatait.");
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  useFocusEffect(
    useCallback(() => {
      void loadSummary();
    }, [loadSummary])
  );

  async function handleLogout() {
    await logout();
  }

  return (
    <ScrollView
      contentContainerStyle={styles.container}
      style={styles.screen}
    >
      <View style={styles.header}>
        <View>
          <Text style={styles.brand}>Logly</Text>
          <Text style={styles.greeting}>Szia, {displayName}!</Text>
          <Text style={styles.subtitle}>Ez a havi összesítésed.</Text>
        </View>

        <Pressable
          accessibilityRole="button"
          onPress={() => void loadSummary()}
          style={({ pressed }) => [
            styles.refreshButton,
            pressed ? styles.pressed : null,
          ]}
        >
          <Text style={styles.refreshButtonText}>Frissítés</Text>
        </Pressable>
      </View>

      {isLoading ? (
        <View style={styles.stateContainer}>
          <ActivityIndicator color="#365CF5" size="large" />
          <Text style={styles.stateText}>Dashboard betöltése…</Text>
        </View>
      ) : errorMessage ? (
        <View style={styles.errorCard}>
          <Text style={styles.errorTitle}>Nem sikerült betölteni</Text>
          <Text style={styles.errorMessage}>{errorMessage}</Text>

          <Pressable
            accessibilityRole="button"
            onPress={() => void loadSummary()}
            style={styles.retryButton}
          >
            <Text style={styles.retryButtonText}>Újrapróbálom</Text>
          </Pressable>
        </View>
      ) : summary ? (
        <>
          <View style={styles.profitCard}>
            <Text style={styles.profitLabel}>Havi nettó eredmény</Text>
            <Text
              style={[
                styles.profitValue,
                summary.netProfit < 0 ? styles.negativeValue : null,
              ]}
            >
              {formatHuf(summary.netProfit)}
            </Text>
            <Text style={styles.periodText}>
              {summary.from} – {summary.to}
            </Text>
          </View>

          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Pénzügyi összesítés</Text>

            <View style={styles.metricsGrid}>
              <MetricCard
                label="Bevétel"
                value={formatHuf(summary.totalRevenue)}
                valueColor="#16803C"
              />
              <MetricCard
                label="Összes kiadás"
                value={formatHuf(summary.totalExpense)}
                valueColor="#D92D20"
              />
              <MetricCard
                label="Üzemanyag"
                value={formatHuf(summary.totalFuelCost)}
              />
              <MetricCard
                label="Munkával kapcsolatos kiadás"
                value={formatHuf(summary.totalWorkExpense)}
              />
              <MetricCard
                label="Egyéb kiadás"
                value={formatHuf(summary.totalOtherExpense)}
              />
            </View>
          </View>

          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Munka összesen</Text>

            <View style={styles.metricsGrid}>
              <MetricCard
                label="Ledolgozott idő"
                value={`${Math.floor(summary.totalWorkedMinutes / 60)} óra ${
                  summary.totalWorkedMinutes % 60
                } perc`}
              />
              <MetricCard
                label="Megtett út"
                value={`${formatNumber(summary.totalDrivenKilometers, 1)} km`}
              />
              <MetricCard
                label="Kiszállítások"
                value={formatNumber(summary.totalDeliveredOrders, 0)}
              />
              <MetricCard
                label="Bevétel óránként"
                value={formatHuf(summary.revenuePerHour)}
              />
              <MetricCard
                label="Nettó óránként"
                value={formatHuf(summary.profitPerHour)}
              />
              <MetricCard
                label="Bevétel kiszállításonként"
                value={formatHuf(summary.revenuePerOrder)}
              />
              <MetricCard
                label="Nettó kiszállításonként"
                value={formatHuf(summary.profitPerOrder)}
              />
              <MetricCard
                label="Teljes költség kilométerenként"
                value={formatHuf(summary.totalCostPerKilometer)}
              />
            </View>
          </View>

          {!summary.includesFuelCost ? (
            <Text style={styles.note}>
              A nettó eredmény nem tartalmaz üzemanyagköltséget, mert ehhez
              még nincs elegendő tankolási adat.
            </Text>
          ) : null}
        </>
      ) : (
        <View style={styles.stateContainer}>
          <Text style={styles.stateText}>
            Ehhez az időszakhoz még nincs megjeleníthető adat.
          </Text>
        </View>
      )}

      <Pressable
        accessibilityRole="button"
        onPress={handleLogout}
        style={({ pressed }) => [
          styles.logoutButton,
          pressed ? styles.pressed : null,
        ]}
      >
        <Text style={styles.logoutButtonText}>Kijelentkezés</Text>
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: "#F6F7FB",
  },
  container: {
    flexGrow: 1,
    padding: 24,
    paddingBottom: 36,
    width: "100%",
    maxWidth: 1100,
    alignSelf: "center",
  },
  header: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 24,
  },
  brand: {
    color: "#1B1D28",
    fontSize: 15,
    fontWeight: "700",
    marginBottom: 24,
  },
  greeting: {
    color: "#1B1D28",
    fontSize: 28,
    fontWeight: "700",
  },
  subtitle: {
    color: "#666B7A",
    fontSize: 15,
    marginTop: 6,
  },
  refreshButton: {
    alignItems: "center",
    backgroundColor: "#FFFFFF",
    borderColor: "#D9DCE5",
    borderRadius: 10,
    borderWidth: 1,
    justifyContent: "center",
    minHeight: 42,
    paddingHorizontal: 14,
  },
  refreshButtonText: {
    color: "#365CF5",
    fontSize: 14,
    fontWeight: "600",
  },
  profitCard: {
    backgroundColor: "#365CF5",
    borderRadius: 18,
    marginBottom: 26,
    padding: 22,
  },
  profitLabel: {
    color: "#E5EAFF",
    fontSize: 15,
    fontWeight: "600",
  },
  profitValue: {
    color: "#FFFFFF",
    fontSize: 34,
    fontWeight: "700",
    marginTop: 8,
  },
  negativeValue: {
    color: "#FFE0DC",
  },
  periodText: {
    color: "#E5EAFF",
    fontSize: 13,
    marginTop: 8,
  },
  section: {
    marginBottom: 24,
  },
  sectionTitle: {
    color: "#1B1D28",
    fontSize: 19,
    fontWeight: "700",
    marginBottom: 12,
  },
  metricsGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 12,
  },
  metricCard: {
    backgroundColor: "#FFFFFF",
    borderColor: "#E7E9F0",
    borderRadius: 14,
    borderWidth: 1,
    flexBasis: 220,
    flexGrow: 1,
    minHeight: 88,
    padding: 16,
  },
  metricLabel: {
    color: "#666B7A",
    fontSize: 13,
    lineHeight: 18,
  },
  metricValue: {
    color: "#1B1D28",
    fontSize: 19,
    fontWeight: "700",
    marginTop: 7,
  },
  stateContainer: {
    alignItems: "center",
    backgroundColor: "#FFFFFF",
    borderRadius: 14,
    justifyContent: "center",
    minHeight: 180,
    padding: 24,
  },
  stateText: {
    color: "#666B7A",
    fontSize: 15,
    marginTop: 12,
    textAlign: "center",
  },
  errorCard: {
    backgroundColor: "#FFF4F2",
    borderColor: "#F5C2BC",
    borderRadius: 14,
    borderWidth: 1,
    padding: 18,
  },
  errorTitle: {
    color: "#9E251B",
    fontSize: 17,
    fontWeight: "700",
  },
  errorMessage: {
    color: "#6B2923",
    fontSize: 14,
    lineHeight: 20,
    marginTop: 8,
  },
  retryButton: {
    alignSelf: "flex-start",
    backgroundColor: "#FFFFFF",
    borderColor: "#D92D20",
    borderRadius: 9,
    borderWidth: 1,
    marginTop: 14,
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  retryButtonText: {
    color: "#D92D20",
    fontSize: 14,
    fontWeight: "700",
  },
  note: {
    color: "#666B7A",
    fontSize: 13,
    lineHeight: 19,
    marginBottom: 20,
  },
  logoutButton: {
    alignItems: "center",
    borderColor: "#D92D20",
    borderRadius: 12,
    borderWidth: 1,
    justifyContent: "center",
    marginTop: 12,
    minHeight: 50,
  },
  logoutButtonText: {
    color: "#D92D20",
    fontSize: 15,
    fontWeight: "700",
  },
  pressed: {
    opacity: 0.75,
  },
});