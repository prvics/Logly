import { useEffect, useRef } from "react";
import {
    Animated,
    Easing,
    Platform,
    StyleSheet,
    type ViewStyle,
} from "react-native";

import { useTheme } from "../../theme/ThemeProvider";

type SkeletonProps = {
    width?: number | `${number}%`;
    height?: number;
    borderRadius?: number;
    color?: string;
    style?: ViewStyle;
};

const USE_NATIVE_DRIVER = Platform.OS !== "web";

export function Skeleton({
    width = "100%",
    height = 16,
    borderRadius = 8,
    color,
    style,
}: SkeletonProps) {
    const { colors } = useTheme();

    const opacity = useRef(
        new Animated.Value(0.45)
    ).current;

    useEffect(() => {
        const animation = Animated.loop(
            Animated.sequence([
                Animated.timing(opacity, {
                    toValue: 0.85,
                    duration: 700,
                    easing: Easing.inOut(Easing.quad),
                    useNativeDriver: USE_NATIVE_DRIVER,
                }),
                Animated.timing(opacity, {
                    toValue: 0.45,
                    duration: 700,
                    easing: Easing.inOut(Easing.quad),
                    useNativeDriver: USE_NATIVE_DRIVER,
                }),
            ])
        );

        animation.start();

        return () => {
            animation.stop();
        };
    }, [opacity]);

    return (
        <Animated.View
            style={[
                styles.skeleton,
                {
                    backgroundColor:
                        color ?? colors.skeleton,
                    width,
                    height,
                    borderRadius,
                    opacity,
                },
                style,
            ]}
        />
    );
}

const styles = StyleSheet.create({
    skeleton: {
        overflow: "hidden",
    },
});