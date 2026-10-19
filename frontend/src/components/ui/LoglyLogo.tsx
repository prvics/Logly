import { View, type StyleProp, type ViewStyle } from "react-native";
import Svg, {
    Circle,
    Path,
    Rect,
    Text as SvgText,
} from "react-native-svg";

type LoglyLogoProps = {
    width?: number;
    style?: StyleProp<ViewStyle>;
};

export function LoglyLogo({
    width = 240,
    style,
}: LoglyLogoProps) {
    const height = width * (96 / 320);

    return (
        <View
            accessible
            accessibilityRole="image"
            accessibilityLabel="Logly"
            style={[{ width, height }, style]}
        >
            <Svg
                width="100%"
                height="100%"
                viewBox="0 0 320 96"
                accessible={false}
            >
                <Rect
                    x={8}
                    y={8}
                    width={80}
                    height={80}
                    rx={22}
                    fill="#365CF5"
                />

                <Path
                    d="M34 30 V58 Q34 66 42 66 H65"
                    fill="none"
                    stroke="#FFFFFF"
                    strokeWidth={7}
                    strokeLinecap="round"
                    strokeLinejoin="round"
                />

                <Circle
                    cx={34}
                    cy={30}
                    r={6}
                    fill="#FFFFFF"
                />

                <Circle
                    cx={65}
                    cy={66}
                    r={6}
                    fill="#365CF5"
                    stroke="#FFFFFF"
                    strokeWidth={3}
                />

                <SvgText
                    x={108}
                    y={64}
                    fill="#1B1D28"
                    fontFamily="Arial"
                    fontSize={48}
                    fontWeight="700"
                    letterSpacing={-2}
                >
                    Logly
                </SvgText>
            </Svg>
        </View>
    );
}