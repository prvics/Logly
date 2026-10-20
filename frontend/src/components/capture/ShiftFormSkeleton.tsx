import {
    StyleSheet,
    View,
} from "react-native";

import { Skeleton } from "../ui/Skeleton";
import {
    ChoiceFieldSkeleton,
    FieldSkeleton,
    HelpTextSkeleton,
} from "./FormSkeletonParts";

type Props = {
    companyCount?: number;
    vehicleCount?: number;
};

export function ShiftFormSkeleton({
    companyCount = 3,
    vehicleCount = 1,
}: Props) {
    return (
        <View
            accessibilityLabel="Műszak adatainak betöltése"
            accessibilityState={{ busy: true }}
            style={styles.form}
        >
            <ChoiceFieldSkeleton
                label="Melyik cégnek dolgoztál?"
                count={companyCount}
                withDescription
            />

            <ChoiceFieldSkeleton
                label="Melyik járművel?"
                count={vehicleCount}
            />

            <FieldSkeleton
                label="Műszak dátuma"
                labelWidth="35%"
            />

            <View style={styles.row}>
                <View style={styles.column}>
                    <FieldSkeleton
                        label="Munkaidő – óra"
                        labelWidth="70%"
                    />
                </View>

                <View style={styles.column}>
                    <FieldSkeleton
                        label="Munkaidő – perc"
                        labelWidth="75%"
                    />
                </View>
            </View>

            <FieldSkeleton
                label="Bevétel (Ft)"
                labelWidth="30%"
            />

            <FieldSkeleton
                label="Kiszállítások száma"
                labelWidth="45%"
            />

            <FieldSkeleton
                label="Megtett út (km)"
                labelWidth="35%"
            />

            <FieldSkeleton
                label="Műszakhoz tartozó kiadás (Ft)"
                labelWidth="65%"
            />

            <HelpTextSkeleton
                text={
                    "Ugyanazt a kiadást ne rögzítsd itt és külön kiadásként is, " +
                    "mert a jelenlegi dashboard mindkettőt összeadja."
                }
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
    );
}

const styles = StyleSheet.create({
    form: {
        gap: 18,
    },
    row: {
        flexDirection: "row",
        gap: 12,
    },
    column: {
        flex: 1,
        minWidth: 0,
    },
});