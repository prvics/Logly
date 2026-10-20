import { useEffect, useRef } from "react";
import {
    Animated,
    Easing,
    Platform,
    Pressable,
    StyleSheet,
    View,
} from "react-native";

import { useTheme } from "../../theme/ThemeProvider";

type ThemeSwitchProps = {
    value: boolean;
    onValueChange: (value: boolean) => void;
    disabled?: boolean;
    accessibilityLabel?: string;
};

const TRACK_WIDTH = 56;
const TRACK_HEIGHT = 32;
const THUMB_SIZE = 26;
const THUMB_MARGIN = 3;

const USE_NATIVE_DRIVER = Platform.OS !== "web";

export function ThemeSwitch({
    value,
    onValueChange,
    disabled = false,
    accessibilityLabel = "Sötét mód kapcsoló",
}: ThemeSwitchProps) {
    const { colors } = useTheme();

    const progress = useRef(
        new Animated.Value(value ? 1 : 0)
    ).current;

    useEffect(() => {
        Animated.timing(progress, {
            toValue: value ? 1 : 0,
            duration: 240,
            easing: Easing.out(Easing.cubic),
            useNativeDriver: USE_NATIVE_DRIVER,
        }).start();
    }, [progress, value]);

    const thumbTranslateX = progress.interpolate({
        inputRange: [0, 1],
        outputRange: [
            0,
            TRACK_WIDTH - THUMB_SIZE - THUMB_MARGIN * 2,
        ],
        extrapolate: "clamp",
    });

    return (
        <Pressable
            accessibilityRole="switch"
            accessibilityLabel={accessibilityLabel}
            accessibilityState={{
                checked: value,
                disabled,
            }}
            disabled={disabled}
            onPress={() => onValueChange(!value)}
            style={({ pressed }) => [
                styles.track,
                {
                    backgroundColor: value
                        ? colors.switchTrackOn
                        : colors.switchTrackOff,
                    opacity: disabled ? 0.55 : pressed ? 0.9 : 1,
                },
            ]}
        >
            <Animated.View
                style={[
                    styles.thumb,
                    {
                        backgroundColor: colors.switchThumb,
                        transform: [
                            {
                                translateX: thumbTranslateX,
                            },
                        ],
                    },
                ]}
            />
        </Pressable>
    );
}

const styles = StyleSheet.create({
    track: {
        alignItems: "center",
        borderRadius: TRACK_HEIGHT / 2,
        height: TRACK_HEIGHT,
        justifyContent: "center",
        width: TRACK_WIDTH,
    },
    thumb: {
        borderRadius: THUMB_SIZE / 2,
        height: THUMB_SIZE,
        left: THUMB_MARGIN,
        position: "absolute",
        shadowColor: "#000000",
        shadowOffset: {
            height: 1,
            width: 0,
        },
        shadowOpacity: 0.18,
        shadowRadius: 3,
        width: THUMB_SIZE,
        elevation: 2,
    },
});