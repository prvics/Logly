import { useEffect, useRef, type ReactNode } from "react";
import {
    Animated,
    Easing,
    Platform,
    type StyleProp,
    type ViewStyle,
} from "react-native";

type FadeInViewProps = {
    children: ReactNode;
    style?: StyleProp<ViewStyle>;
    duration?: number;
    translateY?: number;
};

const USE_NATIVE_DRIVER = Platform.OS !== "web";

export function FadeInView({
    children,
    style,
    duration = 220,
    translateY = 8,
}: FadeInViewProps) {
    const opacity = useRef(
        new Animated.Value(0)
    ).current;

    const animatedTranslateY = useRef(
        new Animated.Value(translateY)
    ).current;

    useEffect(() => {
        const animation = Animated.parallel([
            Animated.timing(opacity, {
                toValue: 1,
                duration,
                easing: Easing.out(Easing.cubic),
                useNativeDriver: USE_NATIVE_DRIVER,
            }),
            Animated.timing(animatedTranslateY, {
                toValue: 0,
                duration,
                easing: Easing.out(Easing.cubic),
                useNativeDriver: USE_NATIVE_DRIVER,
            }),
        ]);

        animation.start();

        return () => {
            animation.stop();
        };
    }, [
        duration,
        opacity,
        animatedTranslateY,
        translateY,
    ]);

    return (
        <Animated.View
            style={[
                style,
                {
                    opacity,
                    transform: [
                        {
                            translateY: animatedTranslateY,
                        },
                    ],
                },
            ]}
        >
            {children}
        </Animated.View>
    );
}