import {
    Pressable,
    StyleSheet,
    Text,
    View,
} from "react-native";

import { useTheme } from "../../theme/ThemeProvider";

export type ChoiceOption = {
    value: string;
    label: string;
    description?: string;
};

type Props = {
    label: string;
    options: ChoiceOption[];
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    error?: string | null;
};

export function ChoiceField({
    label,
    options,
    value,
    onChange,
    disabled = false,
    error,
}: Props) {
    const { colors } = useTheme();

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

            {options.map((option) => {
                const selected = option.value === value;

                return (
                    <Pressable
                        key={option.value}
                        accessibilityRole="radio"
                        accessibilityState={{
                            checked: selected,
                            disabled,
                        }}
                        disabled={disabled}
                        onPress={() => onChange(option.value)}
                        style={({ pressed }) => [
                            styles.option,
                            {
                                backgroundColor: selected
                                    ? colors.surfaceMuted
                                    : colors.surface,
                                borderColor: selected
                                    ? colors.primary
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
                                styles.optionLabel,
                                {
                                    color: selected
                                        ? colors.primary
                                        : colors.textPrimary,
                                },
                            ]}
                        >
                            {selected ? "● " : "○ "}
                            {option.label}
                        </Text>

                        {option.description ? (
                            <Text
                                style={[
                                    styles.description,
                                    { color: colors.textSecondary },
                                ]}
                            >
                                {option.description}
                            </Text>
                        ) : null}
                    </Pressable>
                );
            })}

            {error ? (
                <Text
                    accessibilityLiveRegion="polite"
                    style={[
                        styles.error,
                        { color: colors.danger },
                    ]}
                >
                    {error}
                </Text>
            ) : null}
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        gap: 8,
    },
    label: {
        fontSize: 14,
        fontWeight: "600",
    },
    option: {
        borderRadius: 12,
        borderWidth: 1,
        minHeight: 52,
        padding: 14,
    },
    optionLabel: {
        fontSize: 15,
        fontWeight: "600",
    },
    description: {
        fontSize: 12,
        lineHeight: 18,
        marginTop: 5,
    },
    error: {
        fontSize: 13,
    },
    pressed: {
        opacity: 0.85,
    },
    disabled: {
        opacity: 0.6,
    },
});