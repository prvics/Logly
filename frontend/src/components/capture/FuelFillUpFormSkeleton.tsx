import {
    StyleSheet,
    View,
} from "react-native";

import { Skeleton } from "../ui/Skeleton";
import {
    ChoiceFieldSkeleton,
    FieldSkeleton,
    FullTankSwitchSkeleton,
    HelpTextSkeleton,
} from "./FormSkeletonParts";

type Props = {
    vehicleCount?: number;
};

export function FuelFillUpFormSkeleton({
    vehicleCount = 1,
}: Props) {
    return (
        <View
            accessibilityLabel="Tankolás adatainak betöltése"
            accessibilityState={{ busy: true }}
            style={styles.form}
        >
            <ChoiceFieldSkeleton
                label="Melyik járművet tankoltad?"
                count={vehicleCount}
            />

            <FieldSkeleton
                label="Tankolás dátuma"
                labelWidth="40%"
            />

            <FieldSkeleton
                label="Kilométeróra-állás (km)"
                labelWidth="55%"
            />

            <FieldSkeleton
                label="Tankolt mennyiség (liter)"
                labelWidth="55%"
            />

            <FieldSkeleton
                label="Teljes ár (Ft)"
                labelWidth="30%"
            />

            <FullTankSwitchSkeleton />

            <FieldSkeleton
                label="Megjegyzés"
                labelWidth="30%"
                multiline
            />

            <HelpTextSkeleton
                text={
                    "A tankolás árát ne rögzítsd külön kiadásként vagy " +
                    "műszakkiadásként is, mert a jelenlegi dashboard " +
                    "összeadja ezeket."
                }
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
});