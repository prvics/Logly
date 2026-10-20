import { useState } from "react";
import {
    StyleSheet,
    Text,
    View,
} from "react-native";

import { useTheme } from "../../theme/ThemeProvider";
import { Skeleton } from "../ui/Skeleton";

type LabelProps = {
    label: string;
    width?: number | `${number}%`;
};

export function SkeletonLabel({
    label,
    width = "45%",
}: LabelProps) {
    return (
        <View style={styles.labelContainer}>
            <Text
                accessible={false}
                style={styles.hiddenLabel}
            >
                {label}
            </Text>

            <Skeleton
                width={width}
                height={10}
                borderRadius={5}
                style={styles.labelPlaceholder}
            />
        </View>
    );
}

type FieldProps = {
    label: string;
    labelWidth?: number | `${number}%`;
    multiline?: boolean;
};

export function FieldSkeleton({
    label,
    labelWidth = "45%",
    multiline = false,
}: FieldProps) {
    const { colors } = useTheme();

    return (
        <View style={styles.field}>
            <SkeletonLabel
                label={label}
                width={labelWidth}
            />

            <View
                style={[
                    styles.input,
                    multiline ? styles.multiline : null,
                    {
                        backgroundColor: colors.surface,
                        borderColor: colors.border,
                    },
                ]}
            >
                <Skeleton
                    width={multiline ? "48%" : "38%"}
                    height={12}
                    borderRadius={6}
                />
            </View>
        </View>
    );
}

type ChoiceProps = {
    label: string;
    count?: number;
    withDescription?: boolean;
};

export function ChoiceFieldSkeleton({
    label,
    count = 1,
    withDescription = false,
}: ChoiceProps) {
    const { colors } = useTheme();

    return (
        <View style={styles.choice}>
            <SkeletonLabel
                label={label}
                width="55%"
            />

            {Array.from({ length: count }).map(
                (_, index) => (
                    <View
                        key={index}
                        style={[
                            styles.option,
                            {
                                backgroundColor: colors.surface,
                                borderColor: colors.border,
                            },
                        ]}
                    >
                        <View style={styles.optionTitle}>
                            <Text
                                accessible={false}
                                style={styles.hiddenOptionLabel}
                            >
                                Jármű vagy cég
                            </Text>

                            <View style={styles.optionTitleContent}>
                                <Skeleton
                                    width={10}
                                    height={10}
                                    borderRadius={5}
                                />

                                <Skeleton
                                    width={
                                        index % 2 === 0
                                            ? "28%"
                                            : "42%"
                                    }
                                    height={12}
                                    borderRadius={6}
                                />
                            </View>
                        </View>

                        {withDescription ? (
                            <View style={styles.description}>
                                <Skeleton
                                    width="62%"
                                    height={9}
                                    borderRadius={5}
                                />
                            </View>
                        ) : null}
                    </View>
                )
            )}
        </View>
    );
}

export function HelpTextSkeleton({
    text,
}: {
    text: string;
}) {
    const [lineCount, setLineCount] = useState(1);

    return (
        <View style={styles.helpContainer}>
            <Text
                accessible={false}
                style={styles.hiddenHelp}
                onTextLayout={(event) => {
                    const nextCount = Math.max(
                        1,
                        event.nativeEvent.lines.length
                    );

                    setLineCount((current) =>
                        current === nextCount
                            ? current
                            : nextCount
                    );
                }}
            >
                {text}
            </Text>

            <View style={styles.helpPlaceholders}>
                {Array.from({ length: lineCount }).map(
                    (_, index) => (
                        <View
                            key={index}
                            style={styles.helpLine}
                        >
                            <Skeleton
                                height={8}
                                width={
                                    index === lineCount - 1
                                        ? "72%"
                                        : "95%"
                                }
                                borderRadius={4}
                            />
                        </View>
                    )
                )}
            </View>
        </View>
    );
}

export function FullTankSwitchSkeleton() {
    const { colors } = useTheme();

    return (
        <View
            style={[
                styles.switchRow,
                {
                    backgroundColor: colors.surface,
                    borderColor: colors.border,
                },
            ]}
        >
            <View style={styles.switchText}>
                <SkeletonLabel
                    label="Tele tank"
                    width="30%"
                />

                <HelpTextSkeleton
                    text="Kapcsold be, ha teljesen feltöltötted a tankot."
                />
            </View>

            <View
                style={[
                    styles.switchTrack,
                    {
                        backgroundColor: colors.switchTrackOff,
                    },
                ]}
            >
                <View
                    style={[
                        styles.switchThumb,
                        {
                            backgroundColor: colors.switchThumb,
                        },
                    ]}
                />
            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    labelContainer: {
        justifyContent: "center",
        position: "relative",
    },
    hiddenLabel: {
        fontSize: 14,
        fontWeight: "600",
        opacity: 0,
    },
    labelPlaceholder: {
        position: "absolute",
    },
    field: {
        gap: 6,
    },
    input: {
        borderRadius: 12,
        borderWidth: 1,
        justifyContent: "center",
        minHeight: 52,
        paddingHorizontal: 16,
        paddingVertical: 12,
    },
    multiline: {
        justifyContent: "flex-start",
        minHeight: 100,
        paddingTop: 18,
    },
    choice: {
        gap: 8,
    },
    option: {
        borderRadius: 12,
        borderWidth: 1,
        minHeight: 52,
        padding: 14,
    },
    optionTitle: {
        justifyContent: "center",
        position: "relative",
    },
    hiddenOptionLabel: {
        fontSize: 15,
        fontWeight: "600",
        opacity: 0,
    },
    optionTitleContent: {
        alignItems: "center",
        flexDirection: "row",
        gap: 6,
        left: 0,
        position: "absolute",
        right: 0,
    },
    description: {
        height: 18,
        justifyContent: "center",
        marginTop: 5,
    },
    helpContainer: {
        position: "relative",
    },
    hiddenHelp: {
        fontSize: 13,
        lineHeight: 19,
        opacity: 0,
    },
    helpPlaceholders: {
        position: "absolute",
        top: 0,
        right: 0,
        bottom: 0,
        left: 0,
    },
    helpLine: {
        height: 19,
        justifyContent: "center",
    },
    switchRow: {
        alignItems: "center",
        borderRadius: 12,
        borderWidth: 1,
        flexDirection: "row",
        gap: 16,
        justifyContent: "space-between",
        padding: 16,
    },
    switchText: {
        flex: 1,
        gap: 5,
    },
    switchTrack: {
        borderRadius: 10,
        height: 18,
        justifyContent: "center",
        width: 38,
    },
    switchThumb: {
        borderRadius: 11,
        height: 22,
        left: 0,
        position: "absolute",
        width: 22,
    },
});