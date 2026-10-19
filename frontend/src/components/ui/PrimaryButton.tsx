import {
    ActivityIndicator,
    Pressable,
    StyleSheet,
    Text,
} from "react-native";

type Props = {
    title: string;
    onPress: () => void;
    loading?: boolean;
    disabled?: boolean;
};

export function PrimaryButton({
    title,
    onPress,
    loading = false,
    disabled = false,
}: Props) {
    const isDisabled = loading || disabled;

    return (
        <Pressable
            accessibilityRole="button"
            accessibilityState={{
                disabled: isDisabled,
                busy: loading,
            }}
            disabled={isDisabled}
            onPress={onPress}
            style={({ pressed }) => [
                styles.button,
                pressed && !isDisabled ? styles.pressed : null,
                isDisabled ? styles.disabled : null,
            ]}
        >
            {loading ? (
                <ActivityIndicator color="#FFFFFF" />
            ) : (
                <Text style={styles.text}>{title}</Text>
            )}
        </Pressable>
    );
}

const styles = StyleSheet.create({
    button: {
        alignItems: "center",
        backgroundColor: "#365CF5",
        borderRadius: 12,
        justifyContent: "center",
        minHeight: 52,
        paddingHorizontal: 16,
        paddingVertical: 12,
    },
    text: {
        color: "#FFFFFF",
        fontSize: 16,
        fontWeight: "700",
        textAlign: "center",
    },
    pressed: {
        opacity: 0.85,
    },
    disabled: {
        opacity: 0.6,
    },
});