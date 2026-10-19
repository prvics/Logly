import {
    Pressable,
    StyleSheet,
    Text,
    View,
} from "react-native";

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
    return (
        <View style={styles.container}>
            <Text style={styles.label}>{label}</Text>

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
                            selected ? styles.selectedOption : null,
                            pressed && !disabled ? styles.pressed : null,
                            disabled ? styles.disabled : null,
                        ]}
                    >
                        <Text
                            style={[
                                styles.optionLabel,
                                selected ? styles.selectedLabel : null,
                            ]}
                        >
                            {selected ? "● " : "○ "}
                            {option.label}
                        </Text>

                        {option.description ? (
                            <Text style={styles.description}>
                                {option.description}
                            </Text>
                        ) : null}
                    </Pressable>
                );
            })}

            {error ? (
                <Text
                    accessibilityLiveRegion="polite"
                    style={styles.error}
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
        color: "#2A2D3A",
        fontSize: 14,
        fontWeight: "600",
    },
    option: {
        backgroundColor: "#FFFFFF",
        borderColor: "#D9DCE5",
        borderRadius: 12,
        borderWidth: 1,
        minHeight: 52,
        padding: 14,
    },
    selectedOption: {
        backgroundColor: "#EEF2FF",
        borderColor: "#365CF5",
    },
    optionLabel: {
        color: "#1B1D28",
        fontSize: 15,
        fontWeight: "600",
    },
    selectedLabel: {
        color: "#365CF5",
    },
    description: {
        color: "#666B7A",
        fontSize: 12,
        lineHeight: 18,
        marginTop: 5,
    },
    error: {
        color: "#D92D20",
        fontSize: 13,
    },
    pressed: {
        opacity: 0.85,
    },
    disabled: {
        opacity: 0.6,
    },
});