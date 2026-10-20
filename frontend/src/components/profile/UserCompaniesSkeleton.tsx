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
    companyCount?: number;
    availableCompanyCount?: number;
};

export function UserCompaniesSkeleton({
    companyCount = 2,
    availableCompanyCount = 2,
}: Props) {
    const { colors } = useTheme();

    return (
        <View
            accessibilityLabel="Futárcégek betöltése"
            accessibilityState={{ busy: true }}
            style={styles.section}
        >
            <Text
                style={[
                    styles.heading,
                    { color: colors.textPrimary },
                ]}
            >
                Saját futárcégeim
            </Text>

            {Array.from({ length: companyCount }).map(
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
                        <View style={styles.cardHeader}>
                            <View style={styles.companyName}>
                                <Skeleton
                                    width="70%"
                                    height={16}
                                />
                            </View>

                            <Skeleton
                                width={52}
                                height={24}
                                borderRadius={20}
                            />
                        </View>

                        <View style={styles.editButton}>
                            <Skeleton
                                width={82}
                                height={11}
                            />
                        </View>

                        <HelpTextSkeleton text="Kezdés: 2026-01-01" />
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
                    Meglévő cég hozzáadása
                </Text>

                <ChoiceFieldSkeleton
                    label="Futárcég"
                    count={availableCompanyCount}
                />

                <FieldSkeleton
                    label="Mióta dolgozol ennél a cégnél?"
                    labelWidth="65%"
                />

                <FieldSkeleton
                    label="Megjegyzés"
                    labelWidth="30%"
                    multiline
                />

                <Skeleton
                    height={52}
                    borderRadius={12}
                />
            </View>

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
                    Nem találod a cégedet?
                </Text>

                <HelpTextSkeleton
                    text={
                        "Az új név a közös céglistába kerül. Előbb " +
                        "ellenőrizd, hogy a cég nem szerepel-e már más " +
                        "írásmóddal."
                    }
                />

                <FieldSkeleton
                    label="Új futárcég neve"
                    labelWidth="45%"
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
    cardHeader: {
        alignItems: "center",
        flexDirection: "row",
        gap: 10,
        justifyContent: "space-between",
        minHeight: 24,
    },
    companyName: {
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