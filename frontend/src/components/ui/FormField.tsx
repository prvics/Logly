import {
    StyleSheet,
    Text,
    TextInput,
    View,
} from "react-native";
import type { TextInputProps } from "react-native";

import { useTheme } from "../../theme/ThemeProvider";

type Props = TextInputProps & {
    label: string;
    error?: string | null;
};

export function FormField({
    label,
    error,
    style,
    placeholderTextColor,
    keyboardAppearance,
    ...inputProps
}: Props) {
    const { colors, mode } = useTheme();

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

            <TextInput
                accessibilityLabel={label}
                {...inputProps}
                placeholderTextColor={
                    placeholderTextColor ?? colors.textSecondary
                }
                keyboardAppearance={keyboardAppearance ?? mode}
                selectionColor={colors.primary}
                style={[
                    styles.input,
                    {
                        backgroundColor: colors.surface,
                        borderColor: error
                            ? colors.danger
                            : colors.border,
                        color: colors.textPrimary,
                    },
                    inputProps.multiline
                        ? styles.multiline
                        : null,
                    style,
                ]}
            />

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
        gap: 6,
    },
    label: {
        fontSize: 14,
        fontWeight: "600",
    },
    input: {
        borderRadius: 12,
        borderWidth: 1,
        fontSize: 16,
        minHeight: 52,
        paddingHorizontal: 16,
        paddingVertical: 12,
    },
    multiline: {
        minHeight: 100,
        textAlignVertical: "top",
    },
    error: {
        fontSize: 13,
        lineHeight: 18,
    },
});