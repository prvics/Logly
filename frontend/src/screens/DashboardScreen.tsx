import { useCallback, useState } from "react";
import {
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
import { LoglyLogo } from "../components/ui/LoglyLogo";
import { FadeInView } from "../components/ui/FadeInView";
import { DashboardSkeleton } from "../components/dashboard/DashboardCardSkeleton";
import { useTheme } from "../theme/ThemeProvider";

const formatHuf = (amount: number): string =>
  new Intl.NumberFormat("hu-HU", {
    style: "currency",
    currency: "HUF",
    maximumFractionDigits: 0,
  }).format(amount);

const formatNumber = (
  value: number,
  digits = 1
): string =>
  new Intl.NumberFormat("hu-HU", {
    maximumFractionDigits: digits,
  }).format(value);

function getCurrentMonthRange() {
  const now = new Date();

  const from = new Date(
    now.getFullYear(),
    now.getMonth(),
    1
  );

  const to = new Date(
    now.getFullYear(),
    now.getMonth() + 1,
    0
  );

  const formatDate = (date: Date): string => {
    const year = date.getFullYear();
    const month = String(
      date.getMonth() + 1
    ).padStart(2, "0");
    const day = String(
      date.getDate()
    ).padStart(2, "0");

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
  const { colors } = useTheme();

  return (
    <View
      style={[
        styles.metricCard,
        {
          backgroundColor: colors.surface,
          borderColor: colors.border,
        },
      ]}
    >
      <Text
        style={[
          styles.metricLabel,
          { color: colors.textSecondary },
        ]}
      >
        {label}
      </Text>

      <Text
        style={[
          styles.metricValue,
          {
            color: valueColor ?? colors.textPrimary,
          },
        ]}
      >
        {value}
      </Text>
    </View>
  );
}

export function DashboardScreen() {
  const { colors } = useTheme();

  const user = useAuthStore(
    (state) => state.user
  );

  const logout = useAuthStore(
    (state) => state.logout
  );

  const [summary, setSummary] =
    useState<DashboardSummaryResponse | null>(
      null
    );

  const [isLoading, setIsLoading] =
    useState(true);

  const [errorMessage, setErrorMessage] =
    useState<string | null>(null);

  const displayName =
    user?.displayName?.trim() || "Futár";

  const loadSummary = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const data = await getDashboardSummary(
        getCurrentMonthRange()
      );

      setSummary(data);
    } catch (error) {
      setSummary(null);

      if (error instanceof ApiError) {
        setErrorMessage(error.message);
      } else if (error instanceof Error) {
        setErrorMessage(error.message);
      } else {
        setErrorMessage(
          "Nem sikerült betölteni a dashboard adatait."
        );
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
      style={[
        styles.screen,
        {
          backgroundColor: colors.background,
        },
      ]}
    >
      <View style={styles.header}>
        <View>
          <LoglyLogo
            width={160}
            style={styles.brandLogo}
          />

          <Text
            style={[
              styles.greeting,
              { color: colors.textPrimary },
            ]}
          >
            Szia, {displayName}!
          </Text>

          <Text
            style={[
              styles.subtitle,
              { color: colors.textSecondary },
            ]}
          >
            Ez a havi összesítésed.
          </Text>
        </View>

        <Pressable
          accessibilityRole="button"
          onPress={() => void loadSummary()}
          style={({ pressed }) => [
            styles.refreshButton,
            {
              backgroundColor: colors.surface,
              borderColor: colors.border,
            },
            pressed ? styles.pressed : null,
          ]}
        >
          <Text
            style={[
              styles.refreshButtonText,
              { color: colors.primary },
            ]}
          >
            Frissítés
          </Text>
        </Pressable>
      </View>

      {isLoading ? (
        <DashboardSkeleton />
      ) : errorMessage ? (
        <View
          style={[
            styles.errorCard,
            {
              backgroundColor: colors.surface,
              borderColor: colors.danger,
            },
          ]}
        >
          <Text
            style={[
              styles.errorTitle,
              { color: colors.danger },
            ]}
          >
            Nem sikerült betölteni
          </Text>

          <Text
            style={[
              styles.errorMessage,
              { color: colors.textSecondary },
            ]}
          >
            {errorMessage}
          </Text>

          <Pressable
            accessibilityRole="button"
            onPress={() => void loadSummary()}
            style={[
              styles.retryButton,
              {
                backgroundColor: colors.surface,
                borderColor: colors.danger,
              },
            ]}
          >
            <Text
              style={[
                styles.retryButtonText,
                { color: colors.danger },
              ]}
            >
              Újrapróbálom
            </Text>
          </Pressable>
        </View>
      ) : summary ? (
        <FadeInView translateY={0}>
          <View
            style={[
              styles.profitCard,
              {
                backgroundColor: colors.primary,
              },
            ]}
          >
            <Text style={styles.profitLabel}>
              Havi nettó eredmény
            </Text>

            <Text
              style={[
                styles.profitValue,
                summary.netProfit < 0
                  ? styles.negativeValue
                  : null,
              ]}
            >
              {formatHuf(summary.netProfit)}
            </Text>

            <Text style={styles.periodText}>
              {summary.from} – {summary.to}
            </Text>
          </View>

          <View style={styles.section}>
            <Text
              style={[
                styles.sectionTitle,
                { color: colors.textPrimary },
              ]}
            >
              Pénzügyi összesítés
            </Text>

            <View style={styles.metricsGrid}>
              <MetricCard
                label="Bevétel"
                value={formatHuf(
                  summary.totalRevenue
                )}
                valueColor={colors.success}
              />

              <MetricCard
                label="Összes kiadás"
                value={formatHuf(
                  summary.totalExpense
                )}
                valueColor={colors.danger}
              />

              <MetricCard
                label="Üzemanyag"
                value={formatHuf(
                  summary.totalFuelCost
                )}
              />

              <MetricCard
                label="Munkával kapcsolatos kiadás"
                value={formatHuf(
                  summary.totalWorkExpense
                )}
              />

              <MetricCard
                label="Egyéb kiadás"
                value={formatHuf(
                  summary.totalOtherExpense
                )}
              />
            </View>
          </View>

          <View style={styles.section}>
            <Text
              style={[
                styles.sectionTitle,
                { color: colors.textPrimary },
              ]}
            >
              Munka összesen
            </Text>

            <View style={styles.metricsGrid}>
              <MetricCard
                label="Ledolgozott idő"
                value={`${Math.floor(
                  summary.totalWorkedMinutes /
                  60
                )
                  } óra ${summary.totalWorkedMinutes %
                  60
                  } perc`}
              />

              <MetricCard
                label="Megtett út"
                value={`${formatNumber(
                  summary.totalDrivenKilometers,
                  1
                )} km`}
              />

              <MetricCard
                label="Kiszállítások"
                value={formatNumber(
                  summary.totalDeliveredOrders,
                  0
                )}
              />

              <MetricCard
                label="Bevétel óránként"
                value={formatHuf(
                  summary.revenuePerHour
                )}
              />

              <MetricCard
                label="Nettó óránként"
                value={formatHuf(
                  summary.profitPerHour
                )}
              />

              <MetricCard
                label="Bevétel kiszállításonként"
                value={formatHuf(
                  summary.revenuePerOrder
                )}
              />

              <MetricCard
                label="Nettó kiszállításonként"
                value={formatHuf(
                  summary.profitPerOrder
                )}
              />

              <MetricCard
                label="Teljes költség kilométerenként"
                value={formatHuf(
                  summary.totalCostPerKilometer
                )}
              />
            </View>
          </View>

          {!summary.includesFuelCost ? (
            <Text
              style={[
                styles.note,
                { color: colors.textSecondary },
              ]}
            >
              A nettó eredmény nem tartalmaz
              üzemanyagköltséget, mert ehhez még
              nincs elegendő tankolási adat.
            </Text>
          ) : null}
        </FadeInView>
      ) : (
        <View
          style={[
            styles.stateContainer,
            {
              backgroundColor: colors.surface,
            },
          ]}
        >
          <Text
            style={[
              styles.stateText,
              { color: colors.textSecondary },
            ]}
          >
            Ehhez az időszakhoz még nincs
            megjeleníthető adat.
          </Text>
        </View>
      )}

      <Pressable
        accessibilityRole="button"
        onPress={handleLogout}
        style={({ pressed }) => [
          styles.logoutButton,
          {
            borderColor: colors.danger,
          },
          pressed ? styles.pressed : null,
        ]}
      >
        <Text
          style={[
            styles.logoutButtonText,
            { color: colors.danger },
          ]}
        >
          Kijelentkezés
        </Text>
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
  },
  container: {
    alignSelf: "center",
    flexGrow: 1,
    maxWidth: 1100,
    padding: 24,
    paddingBottom: 36,
    width: "100%",
  },
  header: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 24,
  },
  brandLogo: {
    marginBottom: 16,
  },
  greeting: {
    fontSize: 28,
    fontWeight: "700",
  },
  subtitle: {
    fontSize: 15,
    marginTop: 6,
  },
  refreshButton: {
    alignItems: "center",
    borderRadius: 10,
    borderWidth: 1,
    justifyContent: "center",
    minHeight: 42,
    paddingHorizontal: 14,
  },
  refreshButtonText: {
    fontSize: 14,
    fontWeight: "600",
  },
  profitCard: {
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
    borderRadius: 14,
    borderWidth: 1,
    flexBasis: 220,
    flexGrow: 1,
    minHeight: 88,
    padding: 16,
  },
  metricLabel: {
    fontSize: 13,
    lineHeight: 18,
  },
  metricValue: {
    fontSize: 19,
    fontWeight: "700",
    marginTop: 7,
  },
  stateContainer: {
    alignItems: "center",
    borderRadius: 14,
    justifyContent: "center",
    minHeight: 180,
    padding: 24,
  },
  stateText: {
    fontSize: 15,
    textAlign: "center",
  },
  errorCard: {
    borderRadius: 14,
    borderWidth: 1,
    padding: 18,
  },
  errorTitle: {
    fontSize: 17,
    fontWeight: "700",
  },
  errorMessage: {
    fontSize: 14,
    lineHeight: 20,
    marginTop: 8,
  },
  retryButton: {
    alignSelf: "flex-start",
    borderRadius: 9,
    borderWidth: 1,
    marginTop: 14,
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  retryButtonText: {
    fontSize: 14,
    fontWeight: "700",
  },
  note: {
    fontSize: 13,
    lineHeight: 19,
    marginBottom: 20,
  },
  logoutButton: {
    alignItems: "center",
    borderRadius: 12,
    borderWidth: 1,
    justifyContent: "center",
    marginTop: 12,
    minHeight: 50,
  },
  logoutButtonText: {
    fontSize: 15,
    fontWeight: "700",
  },
  pressed: {
    opacity: 0.75,
  },
});