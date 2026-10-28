import {
    StyleSheet,
    Text,
    View,
} from "react-native";

import { useTheme } from "../../theme/ThemeProvider";
import {
    ChoiceFieldSkeleton,
    FieldSkeleton,
    HelpTextSkeleton,
} from "../capture/FormSkeletonParts";
import { Skeleton } from "../ui/Skeleton";

type Props = {
    itemCount?: number;
};

export function UserVehiclesSkeleton({
    itemCount = 2,
}: Props) {
    const { colors } = useTheme();

    return (
        <View
            accessibilityLabel="Járművek betöltése"
            accessibilityState={{ busy: true }}
            style={styles.section}
        >
            <Text
                style={[
                    styles.heading,
                    { color: colors.textPrimary },
                ]}
            >
                Saját járműveim
            </Text>

            {Array.from({ length: itemCount }).map(
                (_, index) => (
                    <View
                        key={index}
                        style={[
                            styles.card,
                            {
                                backgroundColor: colors.surface,
                                borderColor: colors.border,
                            },
                        ]}
                    >
                        <View style={styles.header}>
                            <View style={styles.name}>
                                <Skeleton width="65%" height={16} />
                            </View>

                            <Skeleton
                                width={52}
                                height={24}
                                borderRadius={20}
                            />
                        </View>

                        <HelpTextSkeleton text="Jármű típusa" />

                        <View style={styles.editButton}>
                            <Skeleton width={82} height={11} />
                        </View>

                        <HelpTextSkeleton text="Kezdő kilométeróra: nincs megadva" />
                    </View>
                )
            )}

            <View
                style={[
                    styles.form,
                    { borderTopColor: colors.border },
                ]}
            >
                <Text
                    style={[
                        styles.subheading,
                        { color: colors.textPrimary },
                    ]}
                >
                    Új jármű hozzáadása
                </Text>

                <FieldSkeleton
                    label="Jármű neve"
                    labelWidth="30%"
                />

                <ChoiceFieldSkeleton
                    label="Jármű típusa"
                    count={6}
                />

                <FieldSkeleton
                    label="Kezdő kilométeróra-állás (km)"
                    labelWidth="65%"
                />

                <Skeleton
                    height={52}
                    borderRadius={12}
                />
            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    section: {
        gap: 16,
        marginTop: 32,
    },
    heading: {
        fontSize: 22,
        fontWeight: "700",
    },
    subheading: {
        fontSize: 17,
        fontWeight: "600",
    },
    card: {
        borderRadius: 14,
        borderWidth: 1,
        gap: 6,
        padding: 16,
    },
    header: {
        alignItems: "center",
        flexDirection: "row",
        gap: 10,
        justifyContent: "space-between",
        minHeight: 24,
    },
    name: {
        flex: 1,
        minWidth: 0,
    },
    editButton: {
        alignSelf: "flex-start",
        justifyContent: "center",
        minHeight: 44,
        paddingHorizontal: 12,
    },
    form: {
        borderTopWidth: 1,
        gap: 14,
        paddingTop: 20,
    },
});