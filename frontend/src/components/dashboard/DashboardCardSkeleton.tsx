import { StyleSheet, View } from "react-native";

import { useTheme } from "../../theme/ThemeProvider";
import { Skeleton } from "../ui/Skeleton";

export function DashboardSkeleton() {
    const { colors } = useTheme();

    return (
        <View style={styles.container}>
            <View
                style={[
                    styles.profitCard,
                    {
                        backgroundColor: colors.primary,
                    },
                ]}
            >
                <Skeleton
                    height={14}
                    width="45%"
                    color="#FFFFFF"
                />

                <Skeleton
                    height={34}
                    width="65%"
                    borderRadius={10}
                    color="#FFFFFF"
                    style={styles.profitValue}
                />

                <Skeleton
                    height={13}
                    width="35%"
                    color="#FFFFFF"
                />
            </View>

            <View style={styles.section}>
                <Skeleton height={18} width="45%" />

                <View style={styles.metricsGrid}>
                    {Array.from({ length: 5 }).map(
                        (_, index) => (
                            <View
                                key={`finance-${index}`}
                                style={[
                                    styles.metricCard,
                                    {
                                        backgroundColor:
                                            colors.surface,
                                        borderColor: colors.border,
                                    },
                                ]}
                            >
                                <Skeleton
                                    height={13}
                                    width="70%"
                                />

                                <Skeleton
                                    height={20}
                                    width="85%"
                                    borderRadius={8}
                                    style={styles.metricValue}
                                />
                            </View>
                        )
                    )}
                </View>
            </View>

            <View style={styles.section}>
                <Skeleton height={18} width="40%" />

                <View style={styles.metricsGrid}>
                    {Array.from({ length: 8 }).map(
                        (_, index) => (
                            <View
                                key={`work-${index}`}
                                style={[
                                    styles.metricCard,
                                    {
                                        backgroundColor:
                                            colors.surface,
                                        borderColor: colors.border,
                                    },
                                ]}
                            >
                                <Skeleton
                                    height={13}
                                    width="70%"
                                />

                                <Skeleton
                                    height={20}
                                    width="85%"
                                    borderRadius={8}
                                    style={styles.metricValue}
                                />
                            </View>
                        )
                    )}
                </View>
            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        gap: 24,
    },
    profitCard: {
        borderRadius: 18,
        marginBottom: 2,
        padding: 22,
    },
    profitValue: {
        marginVertical: 12,
    },
    section: {
        gap: 12,
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
    metricValue: {
        marginTop: 10,
    },
});