import { StyleSheet, Text, View } from "react-native";

type Props = {
    message: string;
    variant?: "error" | "success";
};

export function FeedbackMessage({
    message,
    variant = "error",
}: Props) {
    const isError = variant === "error";

    return (
        <View
            accessibilityLiveRegion="polite"
            style={[
                styles.container,
                isError ? styles.errorBox : styles.successBox,
            ]}
        >
            <Text
                style={[
                    styles.text,
                    isError ? styles.errorText : styles.successText,
                ]}
            >
                {message}
            </Text>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        borderRadius: 12,
        borderWidth: 1,
        padding: 14,
    },
    text: {
        fontSize: 14,
        lineHeight: 21,
    },
    errorBox: {
        backgroundColor: "#FFF4F2",
        borderColor: "#F5C2BC",
    },
    successBox: {
        backgroundColor: "#ECFDF3",
        borderColor: "#ABEFC6",
    },
    errorText: {
        color: "#9E251B",
    },
    successText: {
        color: "#067647",
    },
});