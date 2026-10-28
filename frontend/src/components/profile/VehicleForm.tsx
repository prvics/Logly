import {
    useEffect,
    useRef,
    useState,
} from "react";
import {
    Pressable,
    StyleSheet,
    Switch,
    Text,
    View,
} from "react-native";

import { ApiError } from "../../api/client";
import { useTheme } from "../../theme/ThemeProvider";
import {
    VehicleType,
    VehicleTypeLabels,
} from "../../types/api";
import type {
    UpdateVehicleRequest,
    VehicleResponse,
} from "../../types/api";

import { ChoiceField } from "../ui/ChoiceField";
import { FeedbackMessage } from "../ui/FeedbackMessage";
import { FormField } from "../ui/FormField";
import { PrimaryButton } from "../ui/PrimaryButton";

type Props = {
    vehicle?: VehicleResponse;
    disabled?: boolean;
    onSave: (request: UpdateVehicleRequest) => Promise<void>;
    onCancel?: () => void;
};

type FormErrors = {
    name?: string;
    type?: string;
    startingOdometerKm?: string;
};

const VEHICLE_TYPES: VehicleType[] = [
    VehicleType.Bicycle,
    VehicleType.ElectricBicycle,
    VehicleType.Scooter,
    VehicleType.Motorcycle,
    VehicleType.Car,
    VehicleType.Other,
];

const TYPE_OPTIONS = VEHICLE_TYPES.map((type) => ({
    value: String(type),
    label: VehicleTypeLabels[type],
}));

export function VehicleForm({
    vehicle,
    disabled = false,
    onSave,
    onCancel,
}: Props) {
    const { colors } = useTheme();

    const mountedRef = useRef(false);
    const submittingRef = useRef(false);

    const [name, setName] = useState(
        () => vehicle?.name ?? ""
    );

    const [type, setType] = useState(
        () => vehicle ? String(vehicle.type) : ""
    );

    const [startingOdometerKm, setStartingOdometerKm] =
        useState(() =>
            vehicle?.startingOdometerKm != null
                ? String(vehicle.startingOdometerKm).replace(".", ",")
                : ""
        );

    const [isActive, setIsActive] = useState(
        () => vehicle?.isActive ?? true
    );

    const [errors, setErrors] = useState<FormErrors>({});
    const [formError, setFormError] =
        useState<string | null>(null);

    const [isSubmitting, setIsSubmitting] = useState(false);

    const isEditing = vehicle != null;
    const isDisabled = disabled || isSubmitting;

    useEffect(() => {
        mountedRef.current = true;

        return () => {
            mountedRef.current = false;
        };
    }, []);

    function clearFieldError(field: keyof FormErrors) {
        setErrors((current) => ({
            ...current,
            [field]: undefined,
        }));

        setFormError(null);
    }

    function buildRequest(): UpdateVehicleRequest | null {
        const nextErrors: FormErrors = {};

        const trimmedName = name.trim();
        const selectedType = VEHICLE_TYPES.find(
            (item) => String(item) === type
        );

        if (!trimmedName || trimmedName.length > 100) {
            nextErrors.name =
                "Adj meg egy 1–100 karakteres járműnevet.";
        }

        if (selectedType === undefined) {
            nextErrors.type = "Válassz járműtípust.";
        }

        const normalizedOdometer = startingOdometerKm
            .trim()
            .replace(",", ".");

        let parsedOdometer: number | null = null;

        if (normalizedOdometer) {
            if (!/^\d+(?:\.\d)?$/.test(normalizedOdometer)) {
                nextErrors.startingOdometerKm =
                    "Nem negatív számot adj meg, legfeljebb egy tizedesjeggyel.";
            } else {
                const parsed = Number(normalizedOdometer);

                if (
                    !Number.isFinite(parsed) ||
                    parsed < 0 ||
                    parsed > 9_999_999_999.9
                ) {
                    nextErrors.startingOdometerKm =
                        "Az érték 0 és 9999999999,9 km között legyen.";
                } else {
                    parsedOdometer = parsed;
                }
            }
        }

        setErrors(nextErrors);

        if (
            Object.keys(nextErrors).length > 0 ||
            selectedType === undefined
        ) {
            return null;
        }

        return {
            name: trimmedName,
            type: selectedType,
            isActive,
            startingOdometerKm: parsedOdometer,
        };
    }

    async function handleSubmit() {
        if (
            submittingRef.current ||
            disabled ||
            !mountedRef.current
        ) {
            return;
        }

        setFormError(null);

        const request = buildRequest();

        if (!request) {
            return;
        }

        submittingRef.current = true;
        setIsSubmitting(true);

        try {
            await onSave(request);

            if (!mountedRef.current) {
                return;
            }

            if (!isEditing) {
                setName("");
                setType("");
                setStartingOdometerKm("");
                setIsActive(true);
            }

            setErrors({});
        } catch (error) {
            if (!mountedRef.current) {
                return;
            }

            if (error instanceof ApiError) {
                const nextErrors: FormErrors = {};

                const fields: Array<keyof FormErrors> = [
                    "name",
                    "type",
                    "startingOdometerKm",
                ];

                for (const field of fields) {
                    const message = error.fieldError(field);

                    if (message) {
                        nextErrors[field] = message;
                    }
                }

                setErrors(nextErrors);

                setFormError(
                    Object.keys(nextErrors).length > 0
                        ? "Ellenőrizd a jelölt mezőket."
                        : error.message
                );
            } else {
                setFormError(
                    "Nem sikerült elmenteni a járművet. Próbáld újra."
                );
            }
        } finally {
            submittingRef.current = false;

            if (mountedRef.current) {
                setIsSubmitting(false);
            }
        }
    }

    return (
        <View style={styles.form}>
            <FormField
                label="Jármű neve"
                placeholder="Például: Városi kerékpár"
                value={name}
                onChangeText={(value) => {
                    setName(value);
                    clearFieldError("name");
                }}
                editable={!isDisabled}
                error={errors.name}
            />

            <ChoiceField
                label="Jármű típusa"
                options={TYPE_OPTIONS}
                value={type}
                onChange={(value) => {
                    setType(value);
                    clearFieldError("type");
                }}
                disabled={isDisabled}
                error={errors.type}
            />

            <FormField
                label="Kezdő kilométeróra-állás (km)"
                placeholder="Nem kötelező, például: 125430,5"
                keyboardType="decimal-pad"
                value={startingOdometerKm}
                onChangeText={(value) => {
                    setStartingOdometerKm(value);
                    clearFieldError("startingOdometerKm");
                }}
                editable={!isDisabled}
                error={errors.startingOdometerKm}
            />

            {isEditing ? (
                <>
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
                            <Text
                                style={[
                                    styles.switchLabel,
                                    { color: colors.textPrimary },
                                ]}
                            >
                                Aktív jármű
                            </Text>

                            <Text
                                style={[
                                    styles.help,
                                    { color: colors.textSecondary },
                                ]}
                            >
                                Az aktív járművek kiválaszthatók új műszak
                                és tankolás rögzítésekor.
                            </Text>
                        </View>

                        <Switch
                            accessibilityLabel="Aktív jármű"
                            value={isActive}
                            disabled={isDisabled}
                            onValueChange={(value) => {
                                setIsActive(value);
                                setFormError(null);
                            }}
                            trackColor={{
                                false: colors.switchTrackOff,
                                true: colors.switchTrackOn,
                            }}
                            thumbColor={colors.switchThumb}
                        />
                    </View>

                    <Text
                        style={[
                            styles.help,
                            { color: colors.textSecondary },
                        ]}
                    >
                        Az inaktiválás nem törli a járművet, és nem indít
                        törlési műveletet a korábbi műszakokra vagy
                        tankolásokra.
                    </Text>
                </>
            ) : null}

            {formError ? (
                <FeedbackMessage message={formError} />
            ) : null}

            <PrimaryButton
                title={
                    isEditing
                        ? "Módosítások mentése"
                        : "Jármű hozzáadása"
                }
                loading={isSubmitting}
                onPress={() => void handleSubmit()}
            />

            {onCancel ? (
                <Pressable
                    accessibilityRole="button"
                    accessibilityState={{
                        disabled: isDisabled,
                    }}
                    disabled={isDisabled}
                    onPress={onCancel}
                    style={({ pressed }) => [
                        styles.cancelButton,
                        pressed && !isDisabled
                            ? styles.pressed
                            : null,
                        isDisabled ? styles.disabled : null,
                    ]}
                >
                    <Text
                        style={[
                            styles.cancelText,
                            { color: colors.primary },
                        ]}
                    >
                        Mégse
                    </Text>
                </Pressable>
            ) : null}
        </View>
    );
}

const styles = StyleSheet.create({
    form: {
        gap: 14,
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
    switchLabel: {
        fontSize: 14,
        fontWeight: "600",
    },
    help: {
        fontSize: 13,
        lineHeight: 19,
    },
    cancelButton: {
        alignItems: "center",
        alignSelf: "flex-start",
        justifyContent: "center",
        minHeight: 44,
        paddingHorizontal: 12,
    },
    cancelText: {
        fontSize: 14,
        fontWeight: "600",
    },
    pressed: {
        opacity: 0.75,
    },
    disabled: {
        opacity: 0.6,
    },
});