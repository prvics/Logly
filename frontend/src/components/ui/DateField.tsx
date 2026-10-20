import { useState } from "react";
import {
    Modal,
    Pressable,
    ScrollView,
    StyleSheet,
    Text,
    View,
} from "react-native";
import {
    Calendar,
    LocaleConfig,
} from "react-native-calendars";

import { useTheme } from "../../theme/ThemeProvider";

LocaleConfig.locales["hu"] = {
    monthNames: [
        "Január",
        "Február",
        "Március",
        "Április",
        "Május",
        "Június",
        "Július",
        "Augusztus",
        "Szeptember",
        "Október",
        "November",
        "December",
    ],
    monthNamesShort: [
        "Jan.",
        "Feb.",
        "Márc.",
        "Ápr.",
        "Máj.",
        "Jún.",
        "Júl.",
        "Aug.",
        "Szept.",
        "Okt.",
        "Nov.",
        "Dec.",
    ],
    dayNames: [
        "Vasárnap",
        "Hétfő",
        "Kedd",
        "Szerda",
        "Csütörtök",
        "Péntek",
        "Szombat",
    ],
    dayNamesShort: [
        "V",
        "H",
        "K",
        "Sze",
        "Cs",
        "P",
        "Szo",
    ],
    today: "Ma",
};

LocaleConfig.defaultLocale = "hu";

type Props = {
    label: string;
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    error?: string | null;
};

function displayDate(value: string): string {
    if (!value) {
        return "Válassz dátumot";
    }

    return value.replace(/-/g, ". ") + ".";
}

export function DateField({
    label,
    value,
    onChange,
    disabled = false,
    error,
}: Props) {
    const { colors, themeName } = useTheme();

    const [isOpen, setIsOpen] = useState(false);

    function closeCalendar() {
        setIsOpen(false);
    }

    return (
        <View style={styles.container}>
            <Text
                style={[
                    styles.label,
                    { color: colors.textPrimary },
                ]}
            >
                {label}
            </Text>

            <Pressable
                accessibilityRole="button"
                accessibilityLabel={`${label}: ${displayDate(value)}`}
                accessibilityHint="Dátumválasztó naptár megnyitása"
                accessibilityState={{
                    disabled,
                    expanded: isOpen,
                }}
                disabled={disabled}
                onPress={() => setIsOpen(true)}
                style={({ pressed }) => [
                    styles.field,
                    {
                        backgroundColor: colors.surface,
                        borderColor: error
                            ? colors.danger
                            : colors.border,
                    },
                    pressed && !disabled
                        ? styles.pressed
                        : null,
                    disabled ? styles.disabled : null,
                ]}
            >
                <Text
                    style={[
                        styles.value,
                        {
                            color: value
                                ? colors.textPrimary
                                : colors.textSecondary,
                        },
                    ]}
                >
                    {displayDate(value)}
                </Text>

                <Text
                    style={[
                        styles.openText,
                        { color: colors.primary },
                    ]}
                >
                    Naptár
                </Text>
            </Pressable>

            {error ? (
                <Text
                    accessibilityLiveRegion="polite"
                    style={[
                        styles.errorText,
                        { color: colors.danger },
                    ]}
                >
                    {error}
                </Text>
            ) : null}

            <Modal
                visible={isOpen}
                transparent
                animationType="fade"
                onRequestClose={closeCalendar}
            >
                <View style={styles.overlay}>
                    <Pressable
                        accessibilityLabel="Naptár bezárása"
                        accessibilityRole="button"
                        onPress={closeCalendar}
                        style={StyleSheet.absoluteFill}
                    />

                    <View
                        accessibilityViewIsModal
                        style={[
                            styles.modalCard,
                            { backgroundColor: colors.surface },
                        ]}
                    >
                        <ScrollView
                            keyboardShouldPersistTaps="handled"
                            contentContainerStyle={styles.modalContent}
                        >
                            <View style={styles.modalHeader}>
                                <Text
                                    style={[
                                        styles.modalTitle,
                                        { color: colors.textPrimary },
                                    ]}
                                >
                                    {label}
                                </Text>

                                <Pressable
                                    accessibilityRole="button"
                                    onPress={closeCalendar}
                                    style={styles.closeButton}
                                >
                                    <Text
                                        style={[
                                            styles.closeText,
                                            { color: colors.primary },
                                        ]}
                                    >
                                        Bezárás
                                    </Text>
                                </Pressable>
                            </View>

                            {isOpen ? (
                                <Calendar
                                    key={themeName}
                                    current={value || undefined}
                                    firstDay={1}
                                    monthFormat="yyyy. MMMM"
                                    markedDates={
                                        value
                                            ? {
                                                [value]: {
                                                    selected: true,
                                                    selectedColor: colors.primary,
                                                    selectedTextColor: colors.primaryText,
                                                },
                                            }
                                            : {}
                                    }
                                    onDayPress={(day) => {
                                        if (!disabled) {
                                            onChange(day.dateString);
                                        }

                                        closeCalendar();
                                    }}
                                    theme={{
                                        backgroundColor: colors.surface,
                                        calendarBackground: colors.surface,
                                        textSectionTitleColor: colors.textSecondary,
                                        todayTextColor: colors.primary,
                                        dayTextColor: colors.textPrimary,
                                        monthTextColor: colors.textPrimary,
                                        arrowColor: colors.primary,
                                        textDisabledColor: colors.textSecondary,
                                        selectedDayBackgroundColor: colors.primary,
                                        selectedDayTextColor: colors.primaryText,
                                        textDayFontSize: 15,
                                        textMonthFontSize: 17,
                                        textDayHeaderFontSize: 13,
                                    }}
                                />
                            ) : null}
                        </ScrollView>
                    </View>
                </View>
            </Modal>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        gap: 6,
    },
    label: {
        fontSize: 14,
        fontWeight: "600",
    },
    field: {
        alignItems: "center",
        borderRadius: 12,
        borderWidth: 1,
        flexDirection: "row",
        justifyContent: "space-between",
        minHeight: 52,
        paddingHorizontal: 16,
        paddingVertical: 12,
    },
    value: {
        flexShrink: 1,
        fontSize: 16,
    },
    openText: {
        fontSize: 13,
        fontWeight: "600",
        marginLeft: 12,
    },
    errorText: {
        fontSize: 13,
        lineHeight: 18,
    },
    pressed: {
        opacity: 0.85,
    },
    disabled: {
        opacity: 0.6,
    },
    overlay: {
        alignItems: "center",
        backgroundColor: "rgba(0, 0, 0, 0.55)",
        flex: 1,
        justifyContent: "center",
        padding: 20,
    },
    modalCard: {
        borderRadius: 18,
        maxHeight: "90%",
        maxWidth: 420,
        overflow: "hidden",
        width: "100%",
    },
    modalContent: {
        padding: 16,
    },
    modalHeader: {
        alignItems: "center",
        flexDirection: "row",
        gap: 12,
        justifyContent: "space-between",
        marginBottom: 12,
    },
    modalTitle: {
        flex: 1,
        fontSize: 18,
        fontWeight: "700",
    },
    closeButton: {
        justifyContent: "center",
        minHeight: 44,
        paddingHorizontal: 8,
    },
    closeText: {
        fontSize: 14,
        fontWeight: "600",
    },
});