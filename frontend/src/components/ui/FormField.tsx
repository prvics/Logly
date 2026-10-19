import {
    StyleSheet,
    Text,
    TextInput,
    View,
} from "react-native";
import type { TextInputProps } from "react-native";

type Props = TextInputProps & {
    label: string;
    error?: string | null;
};

export function FormField({
    label,
    error,
    style,
    ...inputProps
}: Props) {
    return (
        <View style={styles.container}>
            <Text style={styles.label}>{label}</Text>

            <TextInput
                accessibilityLabel={label}
                placeholderTextColor="#8A8A8A"
                {...inputProps}
                style={[
                    styles.input,
                    inputProps.multiline ? styles.multiline : null,
                    error ? styles.inputError : null,
                    style,
                ]}
            />

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
        gap: 6,
    },
    label: {
        color: "#2A2D3A",
        fontSize: 14,
        fontWeight: "600",
    },
    input: {
        backgroundColor: "#FFFFFF",
        borderColor: "#D9DCE5",
        borderRadius: 12,
        borderWidth: 1,
        color: "#1B1D28",
        fontSize: 16,
        minHeight: 52,
        paddingHorizontal: 16,
        paddingVertical: 12,
    },
    multiline: {
        minHeight: 100,
        textAlignVertical: "top",
    },
    inputError: {
        borderColor: "#D92D20",
    },
    error: {
        color: "#D92D20",
        fontSize: 13,
        lineHeight: 18,
    },
});