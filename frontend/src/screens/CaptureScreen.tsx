import {
  useCallback,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from "react";
import {
  AccessibilityInfo,
  Animated,
  Easing,
  Keyboard,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";

import { ShiftForm } from "../components/capture/ShiftForm";
import { FuelFillUpForm } from "../components/capture/FuelFillUpForm";
import { FadeInView } from "../components/ui/FadeInView";
import { useTheme } from "../theme/ThemeProvider";

type CaptureType = "shift" | "fuel";

const SELECTOR_PADDING = 5;
const SELECTOR_GAP = 6;

const USE_NATIVE_DRIVER = Platform.OS !== "web";

export function CaptureScreen() {
  const { colors } = useTheme();

  const [captureType, setCaptureType] =
    useState<CaptureType>("shift");

  const [displayedType, setDisplayedType] =
    useState<CaptureType>("shift");

  const [fuelOpened, setFuelOpened] = useState(false);
  const [selectorWidth, setSelectorWidth] = useState(0);
  const [isTransitioning, setIsTransitioning] = useState(false);

  const mountedRef = useRef(false);
  const transitioningRef = useRef(false);
  const enterPendingRef = useRef(false);
  const reduceMotionRef = useRef(false);

  const indicatorProgress = useRef(
    new Animated.Value(0)
  ).current;

  const contentOpacity = useRef(
    new Animated.Value(1)
  ).current;

  const contentTranslateX = useRef(
    new Animated.Value(0)
  ).current;

  const buttonWidth = Math.max(
    0,
    (
      selectorWidth -
      SELECTOR_PADDING * 2 -
      SELECTOR_GAP
    ) / 2
  );

  const indicatorTranslateX =
    indicatorProgress.interpolate({
      inputRange: [0, 1],
      outputRange: [
        0,
        buttonWidth + SELECTOR_GAP,
      ],
      extrapolate: "clamp",
    });

  const shiftTextColor =
    indicatorProgress.interpolate({
      inputRange: [0, 1],
      outputRange: [
        colors.primaryText,
        colors.textSecondary,
      ],
      extrapolate: "clamp",
    });

  const fuelTextColor =
    indicatorProgress.interpolate({
      inputRange: [0, 1],
      outputRange: [
        colors.textSecondary,
        colors.primaryText,
      ],
      extrapolate: "clamp",
    });

  useEffect(() => {
    mountedRef.current = true;

    void AccessibilityInfo.isReduceMotionEnabled()
      .then((enabled) => {
        if (mountedRef.current) {
          reduceMotionRef.current = enabled;
        }
      })
      .catch(() => {
        // A beállítás lekérdezése nem akadályozza a képernyőt.
      });

    const subscription =
      AccessibilityInfo.addEventListener(
        "reduceMotionChanged",
        (enabled) => {
          reduceMotionRef.current = enabled;
        }
      );

    return () => {
      mountedRef.current = false;
      enterPendingRef.current = false;
      transitioningRef.current = false;

      subscription.remove();

      indicatorProgress.stopAnimation();
      contentOpacity.stopAnimation();
      contentTranslateX.stopAnimation();
    };
  }, [
    indicatorProgress,
    contentOpacity,
    contentTranslateX,
  ]);

  const finishTransition = useCallback(() => {
    if (!mountedRef.current) {
      return;
    }

    transitioningRef.current = false;
    setIsTransitioning(false);
  }, []);

  useLayoutEffect(() => {
    if (!enterPendingRef.current) {
      return;
    }

    enterPendingRef.current = false;

    if (reduceMotionRef.current) {
      contentOpacity.setValue(1);
      contentTranslateX.setValue(0);
      finishTransition();
      return;
    }

    Animated.parallel([
      Animated.timing(contentOpacity, {
        toValue: 1,
        duration: 170,
        easing: Easing.out(Easing.cubic),
        useNativeDriver: USE_NATIVE_DRIVER,
      }),
      Animated.timing(contentTranslateX, {
        toValue: 0,
        duration: 170,
        easing: Easing.out(Easing.cubic),
        useNativeDriver: USE_NATIVE_DRIVER,
      }),
    ]).start(() => {
      finishTransition();
    });
  }, [
    displayedType,
    contentOpacity,
    contentTranslateX,
    finishTransition,
  ]);

  function selectType(type: CaptureType) {
    if (
      type === captureType ||
      transitioningRef.current ||
      !mountedRef.current
    ) {
      return;
    }

    Keyboard.dismiss();

    if (type === "fuel") {
      setFuelOpened(true);
    }

    const targetProgress =
      type === "shift" ? 0 : 1;

    const direction =
      type === "fuel" ? 1 : -1;

    setCaptureType(type);

    if (reduceMotionRef.current && !__DEV__) {
      indicatorProgress.setValue(targetProgress);
      contentOpacity.setValue(1);
      contentTranslateX.setValue(0);
      setDisplayedType(type);
      return;
    }

    transitioningRef.current = true;
    setIsTransitioning(true);

    Animated.timing(indicatorProgress, {
      toValue: targetProgress,
      duration: 260,
      easing: Easing.inOut(Easing.cubic),
      useNativeDriver: false,
    }).start();

    Animated.parallel([
      Animated.timing(contentOpacity, {
        toValue: 0,
        duration: 90,
        easing: Easing.in(Easing.quad),
        useNativeDriver: USE_NATIVE_DRIVER,
      }),
      Animated.timing(contentTranslateX, {
        toValue: -8 * direction,
        duration: 90,
        easing: Easing.in(Easing.quad),
        useNativeDriver: USE_NATIVE_DRIVER,
      }),
    ]).start(({ finished }) => {
      if (!finished || !mountedRef.current) {
        return;
      }

      contentTranslateX.setValue(12 * direction);
      enterPendingRef.current = true;

      setDisplayedType(type);
    });
  }

  return (
    <KeyboardAvoidingView
      behavior={
        Platform.OS === "ios"
          ? "padding"
          : undefined
      }
      style={[
        styles.screen,
        {
          backgroundColor: colors.background,
        },
      ]}
    >
      <ScrollView
        contentContainerStyle={styles.container}
        keyboardShouldPersistTaps="handled"
      >
        <View style={styles.header}>
          <Text
            style={[
              styles.title,
              { color: colors.textPrimary },
            ]}
          >
            Adatok rögzítése
          </Text>

          <Text
            style={[
              styles.subtitle,
              { color: colors.textSecondary },
            ]}
          >
            Válaszd ki, mit szeretnél rögzíteni.
          </Text>
        </View>

        <View
          style={[
            styles.typeSelector,
            {
              backgroundColor: colors.surfaceMuted,
            },
          ]}
          onLayout={(event) => {
            setSelectorWidth(
              event.nativeEvent.layout.width
            );
          }}
        >
          {selectorWidth > 0 ? (
            <Animated.View
              accessible={false}
              style={[
                styles.activeIndicator,
                {
                  backgroundColor: colors.primary,
                  width: buttonWidth,
                  transform: [
                    {
                      translateX:
                        indicatorTranslateX,
                    },
                  ],
                },
              ]}
            />
          ) : null}

          <Pressable
            accessibilityRole="tab"
            accessibilityLabel="Műszak"
            accessibilityState={{
              selected:
                captureType === "shift",
              disabled: isTransitioning,
            }}
            disabled={isTransitioning}
            onPress={() => selectType("shift")}
            style={({ pressed }) => [
              styles.typeButton,
              pressed && !isTransitioning
                ? styles.typeButtonPressed
                : null,
            ]}
          >
            <Animated.Text
              style={[
                styles.typeButtonText,
                {
                  color: shiftTextColor,
                },
              ]}
            >
              Műszak
            </Animated.Text>
          </Pressable>

          <Pressable
            accessibilityRole="tab"
            accessibilityLabel="Tankolás"
            accessibilityState={{
              selected:
                captureType === "fuel",
              disabled: isTransitioning,
            }}
            disabled={isTransitioning}
            onPress={() => selectType("fuel")}
            style={({ pressed }) => [
              styles.typeButton,
              pressed && !isTransitioning
                ? styles.typeButtonPressed
                : null,
            ]}
          >
            <Animated.Text
              style={[
                styles.typeButtonText,
                {
                  color: fuelTextColor,
                },
              ]}
            >
              Tankolás
            </Animated.Text>
          </Pressable>
        </View>

        <FadeInView>
          <Text
            style={[
              styles.sectionTitle,
              { color: colors.textPrimary },
            ]}
          >
            {displayedType === "shift"
              ? "Műszak rögzítése"
              : "Tankolás rögzítése"}
          </Text>

          <View
            style={
              displayedType !== "shift"
                ? styles.hidden
                : undefined
            }
          >
            <ShiftForm />
          </View>

          {fuelOpened ? (
            <View
              style={
                displayedType !== "fuel"
                  ? styles.hidden
                  : undefined
              }
            >
              <FuelFillUpForm />
            </View>
          ) : null}
        </FadeInView>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
  },
  container: {
    alignSelf: "center",
    flexGrow: 1,
    maxWidth: 680,
    padding: 24,
    paddingBottom: 40,
    width: "100%",
  },
  header: {
    marginBottom: 22,
  },
  title: {
    fontSize: 26,
    fontWeight: "700",
  },
  subtitle: {
    fontSize: 16,
    lineHeight: 23,
    marginTop: 10,
  },
  typeSelector: {
    borderRadius: 12,
    flexDirection: "row",
    gap: SELECTOR_GAP,
    marginBottom: 24,
    padding: SELECTOR_PADDING,
    position: "relative",
  },
  activeIndicator: {
    borderRadius: 9,
    bottom: SELECTOR_PADDING,
    left: SELECTOR_PADDING,
    pointerEvents: "none",
    position: "absolute",
    top: SELECTOR_PADDING,
  },
  typeButton: {
    alignItems: "center",
    borderRadius: 9,
    flex: 1,
    justifyContent: "center",
    minHeight: 44,
    paddingHorizontal: 12,
    zIndex: 1,
  },
  typeButtonPressed: {
    opacity: 0.8,
  },
  typeButtonText: {
    fontSize: 15,
    fontWeight: "600",
  },
  sectionTitle: {
    fontSize: 20,
    fontWeight: "700",
    marginBottom: 20,
  },
  hidden: {
    display: "none",
  },
});