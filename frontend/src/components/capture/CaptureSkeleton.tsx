import { StyleSheet, View } from "react-native";

import { useTheme } from "../../theme/ThemeProvider";
import { Skeleton } from "../ui/Skeleton";

export function CaptureSkeleton() {
    const { colors } = useTheme();

    return (
        <View style={styles.container}>
            <View
                style={[
                    styles.card,
                    {
                        backgroundColor: colors.surface,
                        borderColor: colors.border,
                    },
                ]}
            >
                <Skeleton height={18} width="40%" />

                <View style={styles.fieldGroup}>
                    <Skeleton height={13} width="30%" />
                    <Skeleton
                        height={48}
                        borderRadius={12}
                    />
                </View>

                <View style={styles.fieldGroup}>
                    <Skeleton height={13} width="45%" />
                    <Skeleton
                        height={48}
                        borderRadius={12}
                    />
                </View>

                <View style={styles.row}>
                    <View style={styles.halfField}>
                        <Skeleton height={13} width="55%" />
                        <Skeleton
                            height={48}
                            borderRadius={12}
                        />
                    </View>

                    <View style={styles.halfField}>
                        <Skeleton height={13} width="45%" />
                        <Skeleton
                            height={48}
                            borderRadius={12}
                        />
                    </View>
                </View>

                <Skeleton
                    height={52}
                    borderRadius={12}
                    style={styles.button}
                />
            </View>

            <View
                style={[
                    styles.card,
                    {
                        backgroundColor: colors.surface,
                        borderColor: colors.border,
                    },
                ]}
            >
                <Skeleton height={18} width="35%" />

                <View style={styles.fieldGroup}>
                    <Skeleton height={13} width="50%" />
                    <Skeleton
                        height={48}
                        borderRadius={12}
                    />
                </View>

                <View style={styles.row}>
                    <View style={styles.halfField}>
                        <Skeleton height={13} width="40%" />
                        <Skeleton
                            height={48}
                            borderRadius={12}
                        />
                    </View>

                    <View style={styles.halfField}>
                        <Skeleton height={13} width="55%" />
                        <Skeleton
                            height={48}
                            borderRadius={12}
                        />
                    </View>
                </View>

                <Skeleton
                    height={52}
                    borderRadius={12}
                    style={styles.button}
                />
            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        gap: 20,
    },
    card: {
        borderRadius: 16,
        borderWidth: 1,
        padding: 18,
    },
    fieldGroup: {
        gap: 8,
        marginTop: 18,
    },
    row: {
        flexDirection: "row",
        gap: 12,
        marginTop: 18,
    },
    halfField: {
        flex: 1,
        gap: 8,
    },
    button: {
        marginTop: 22,
    },
});