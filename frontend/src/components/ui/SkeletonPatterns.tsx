import { StyleSheet, View } from "react-native";

import { useTheme } from "../../theme/ThemeProvider";
import { Skeleton } from "./Skeleton";

export function SkeletonText({
    lines = 3,
}: {
    lines?: number;
}) {
    return (
        <View style={styles.gap}>
            {Array.from({ length: lines }).map((_, index) => (
                <Skeleton
                    key={index}
                    height={14}
                    width={
                        index === lines - 1 ? "65%" : "100%"
                    }
                />
            ))}
        </View>
    );
}

export function SkeletonCard() {
    const { colors } = useTheme();

    return (
        <View
            style={[
                styles.card,
                {
                    backgroundColor: colors.surface,
                    borderColor: colors.border,
                },
            ]}
        >
            <Skeleton
                width={44}
                height={44}
                borderRadius={12}
            />

            <View style={styles.cardContent}>
                <Skeleton height={16} width="70%" />
                <Skeleton height={13} width="45%" />
            </View>

            <Skeleton
                width={72}
                height={20}
                borderRadius={10}
            />
        </View>
    );
}

export function SkeletonList({
    itemCount = 4,
}: {
    itemCount?: number;
}) {
    return (
        <View style={styles.gap}>
            {Array.from({ length: itemCount }).map(
                (_, index) => (
                    <SkeletonCard key={index} />
                )
            )}
        </View>
    );
}

export function SkeletonForm() {
    return (
        <View style={styles.gapLarge}>
            <Skeleton height={18} width="40%" />
            <Skeleton height={48} borderRadius={12} />
            <Skeleton height={48} borderRadius={12} />
            <Skeleton height={52} borderRadius={12} />
        </View>
    );
}

const styles = StyleSheet.create({
    gap: {
        gap: 12,
    },
    gapLarge: {
        gap: 18,
    },
    card: {
        alignItems: "center",
        borderRadius: 16,
        borderWidth: 1,
        flexDirection: "row",
        gap: 14,
        padding: 16,
    },
    cardContent: {
        flex: 1,
        gap: 8,
    },
});