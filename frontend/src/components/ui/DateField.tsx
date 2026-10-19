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
    const [isOpen, setIsOpen] = useState(false);

    function closeCalendar() {
        setIsOpen(false);
    }

    return (
        <View style={styles.container}>
            <Text style={styles.label}>{label}</Text>

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
                    error ? styles.fieldError : null,
                    pressed && !disabled ? styles.pressed : null,
                    disabled ? styles.disabled : null,
                ]}
            >
                <Text
                    style={[
                        styles.value,
                        !value ? styles.placeholder : null,
                    ]}
                >
                    {displayDate(value)}
                </Text>

                <Text style={styles.openText}>Naptár</Text>
            </Pressable>

            {error ? (
                <Text
                    accessibilityLiveRegion="polite"
                    style={styles.errorText}
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
                        style={StyleSheet.absoluteFillObject}
                    />

                    <View
                        accessibilityViewIsModal
                        style={styles.modalCard}
                    >
                        <ScrollView
                            keyboardShouldPersistTaps="handled"
                            contentContainerStyle={styles.modalContent}
                        >
                            <View style={styles.modalHeader}>
                                <Text style={styles.modalTitle}>
                                    {label}
                                </Text>

                                <Pressable
                                    accessibilityRole="button"
                                    onPress={closeCalendar}
                                    style={styles.closeButton}
                                >
                                    <Text style={styles.closeText}>
                                        Bezárás
                                    </Text>
                                </Pressable>
                            </View>

                            {isOpen ? (
                                <Calendar
                                    current={value || undefined}
                                    firstDay={1}
                                    monthFormat="yyyy. MMMM"
                                    markedDates={
                                        value
                                            ? {
                                                [value]: {
                                                    selected: true,
                                                    selectedColor: "#365CF5",
                                                    selectedTextColor: "#FFFFFF",
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
                                        calendarBackground: "#FFFFFF",
                                        textSectionTitleColor: "#666B7A",
                                        todayTextColor: "#365CF5",
                                        dayTextColor: "#1B1D28",
                                        monthTextColor: "#1B1D28",
                                        arrowColor: "#365CF5",
                                        textDisabledColor: "#C5C8D0",
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
        color: "#2A2D3A",
        fontSize: 14,
        fontWeight: "600",
    },
    field: {
        alignItems: "center",
        backgroundColor: "#FFFFFF",
        borderColor: "#D9DCE5",
        borderRadius: 12,
        borderWidth: 1,
        flexDirection: "row",
        justifyContent: "space-between",
        minHeight: 52,
        paddingHorizontal: 16,
        paddingVertical: 12,
    },
    fieldError: {
        borderColor: "#D92D20",
    },
    value: {
        color: "#1B1D28",
        fontSize: 16,
    },
    placeholder: {
        color: "#8A8A8A",
    },
    openText: {
        color: "#365CF5",
        fontSize: 13,
        fontWeight: "600",
    },
    errorText: {
        color: "#D92D20",
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
        backgroundColor: "rgba(27, 29, 40, 0.45)",
        flex: 1,
        justifyContent: "center",
        padding: 20,
    },
    modalCard: {
        backgroundColor: "#FFFFFF",
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
        color: "#1B1D28",
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
        color: "#365CF5",
        fontSize: 14,
        fontWeight: "600",
    },
});