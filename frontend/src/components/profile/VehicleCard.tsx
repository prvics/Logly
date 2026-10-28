import type { ReactNode } from "react";
import {
    Pressable,
    StyleSheet,
    Text,
    View,
} from "react-native";

import { useTheme } from "../../theme/ThemeProvider";
import { VehicleTypeLabels } from "../../types/api";
import type { VehicleResponse } from "../../types/api";

type Props = {
    vehicle: VehicleResponse;
    isEditing: boolean;
    disabled?: boolean;
    onEdit: () => void;
    onCancel: () => void;
    children?: ReactNode;
};

const numberFormatter = new Intl.NumberFormat("hu-HU", {
    maximumFractionDigits: 1,
});

export function VehicleCard({
    vehicle,
    isEditing,
    disabled = false,
    onEdit,
    onCancel,
    children,
}: Props) {
    const { colors } = useTheme();

    return (
        <View
            style={[
                styles.card,
                {
                    backgroundColor: colors.surface,
                    borderColor: isEditing
                        ? colors.primary
                        : colors.border,
                },
            ]}
        >
            <View style={styles.header}>
                <Text
                    style={[
                        styles.name,
                        { color: colors.textPrimary },
                    ]}
                >
                    {vehicle.name}
                </Text>

                <View
                    style={[
                        styles.badge,
                        { backgroundColor: colors.surfaceMuted },
                    ]}
                >
                    <Text
                        style={[
                            styles.badgeText,
                            {
                                color: vehicle.isActive
                                    ? colors.success
                                    : colors.textSecondary,
                            },
                        ]}
                    >
                        {vehicle.isActive ? "Aktív" : "Inaktív"}
                    </Text>
                </View>
            </View>

            <Text
                style={[
                    styles.help,
                    { color: colors.textSecondary },
                ]}
            >
                {VehicleTypeLabels[vehicle.type] ?? "Ismeretlen típus"}
            </Text>

            <Pressable
                accessibilityRole="button"
                accessibilityLabel={
                    `${vehicle.name}: ` +
                    (isEditing
                        ? "szerkesztés bezárása"
                        : "jármű szerkesztése")
                }
                accessibilityState={{
                    disabled,
                    expanded: isEditing,
                }}
                disabled={disabled}
                onPress={isEditing ? onCancel : onEdit}
                style={({ pressed }) => [
                    styles.editButton,
                    pressed && !disabled ? styles.pressed : null,
                    disabled ? styles.disabled : null,
                ]}
            >
                <Text
                    style={[
                        styles.editText,
                        { color: colors.primary },
                    ]}
                >
                    {isEditing
                        ? "Szerkesztés bezárása"
                        : "Szerkesztés"}
                </Text>
            </Pressable>

            {isEditing ? (
                <View
                    style={[
                        styles.editForm,
                        { borderTopColor: colors.border },
                    ]}
                >
                    {children}
                </View>
            ) : (
                <Text
                    style={[
                        styles.help,
                        { color: colors.textSecondary },
                    ]}
                >
                    {vehicle.startingOdometerKm != null
                        ? `Kezdő kilométeróra: ${numberFormatter.format(vehicle.startingOdometerKm)
                        } km`
                        : "Kezdő kilométeróra: nincs megadva"}
                </Text>
            )}
        </View>
    );
}

const styles = StyleSheet.create({
    card: {
        borderRadius: 14,
        borderWidth: 1,
        gap: 6,
        padding: 16,
    },
    header: {
        alignItems: "center",
        flexDirection: "row",
        flexWrap: "wrap",
        gap: 10,
        justifyContent: "space-between",
    },
    name: {
        flexShrink: 1,
        fontSize: 17,
        fontWeight: "700",
    },
    badge: {
        borderRadius: 20,
        paddingHorizontal: 10,
        paddingVertical: 4,
    },
    badgeText: {
        fontSize: 12,
        fontWeight: "600",
    },
    help: {
        fontSize: 13,
        lineHeight: 19,
    },
    editButton: {
        alignItems: "center",
        alignSelf: "flex-start",
        justifyContent: "center",
        minHeight: 44,
        paddingHorizontal: 12,
    },
    editText: {
        fontSize: 14,
        fontWeight: "600",
    },
    editForm: {
        borderTopWidth: 1,
        marginTop: 10,
        paddingTop: 16,
    },
    pressed: {
        opacity: 0.75,
    },
    disabled: {
        opacity: 0.6,
    },
});