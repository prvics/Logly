import { useCallback, useRef, useState } from "react";
import {
    StyleSheet,
    Switch,
    Text,
    View,
} from "react-native";
import { useFocusEffect } from "@react-navigation/native";

import { ApiError } from "../../api/client";
import {
    createFuelFillUp,
    getVehicles,
} from "../../api/endpoints";
import type {
    CreateFuelFillUpRequest,
    VehicleResponse,
} from "../../types/api";

import { useTheme } from "../../theme/ThemeProvider";
import { ChoiceField } from "../ui/ChoiceField";
import { DateField } from "../ui/DateField";
import { FeedbackMessage } from "../ui/FeedbackMessage";
import { FormField } from "../ui/FormField";
import { PrimaryButton } from "../ui/PrimaryButton";
import { FuelFillUpFormSkeleton } from "./FuelFillUpFormSkeleton";

type FormValues = {
    vehicleId: string;
    date: string;
    odometerKm: string;
    liters: string;
    totalPrice: string;
    isFullTank: boolean;
    note: string;
};

type FieldName = keyof FormValues;

type FieldErrors = Partial<Record<FieldName, string>>;

function todayLocal(): string {
    const now = new Date();

    return [
        now.getFullYear(),
        String(now.getMonth() + 1).padStart(2, "0"),
        String(now.getDate()).padStart(2, "0"),
    ].join("-");
}

function initialValues(): FormValues {
    return {
        vehicleId: "",
        date: todayLocal(),
        odometerKm: "",
        liters: "",
        totalPrice: "",
        isFullTank: false,
        note: "",
    };
}

function isValidDate(value: string): boolean {
    if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) {
        return false;
    }

    const date = new Date(`${value}T00:00:00Z`);

    return (
        !Number.isNaN(date.getTime()) &&
        date.toISOString().slice(0, 10) === value
    );
}

function parseNonNegative(value: string): number | null {
    const normalized = value.trim().replace(",", ".");

    if (!/^\d+(?:\.\d+)?$/.test(normalized)) {
        return null;
    }

    const parsed = Number(normalized);

    return Number.isFinite(parsed) && parsed >= 0
        ? parsed
        : null;
}

export function FuelFillUpForm() {
    const { colors } = useTheme();

    const submittingRef = useRef(false);

    const [vehicles, setVehicles] =
        useState<VehicleResponse[]>([]);

    const [isLoadingVehicles, setIsLoadingVehicles] =
        useState(true);

    const [vehiclesError, setVehiclesError] =
        useState<string | null>(null);

    const [reloadKey, setReloadKey] = useState(0);

    const [form, setForm] =
        useState<FormValues>(initialValues);

    const [errors, setErrors] =
        useState<FieldErrors>({});

    const [formError, setFormError] =
        useState<string | null>(null);

    const [successMessage, setSuccessMessage] =
        useState<string | null>(null);

    const [isSubmitting, setIsSubmitting] =
        useState(false);

    useFocusEffect(
        useCallback(() => {
            let active = true;

            setIsLoadingVehicles(true);
            setVehiclesError(null);

            async function loadVehicles() {
                try {
                    const data = await getVehicles();

                    if (!active) {
                        return;
                    }

                    setVehicles(
                        data.filter((vehicle) => vehicle.isActive)
                    );
                } catch (error) {
                    if (!active) {
                        return;
                    }

                    setVehiclesError(
                        error instanceof ApiError
                            ? error.message
                            : "Nem sikerült betölteni a járműveket."
                    );
                } finally {
                    if (active) {
                        setIsLoadingVehicles(false);
                    }
                }
            }

            void loadVehicles();

            return () => {
                active = false;
            };
        }, [reloadKey])
    );

    function updateField<K extends FieldName>(
        field: K,
        value: FormValues[K]
    ) {
        setForm((current) => ({
            ...current,
            [field]: value,
        }));

        setErrors((current) => ({
            ...current,
            [field]: undefined,
        }));

        setFormError(null);
        setSuccessMessage(null);
    }

    function buildRequest(): CreateFuelFillUpRequest | null {
        const nextErrors: FieldErrors = {};

        if (
            !vehicles.some(
                (vehicle) => vehicle.id === form.vehicleId
            )
        ) {
            nextErrors.vehicleId = "Válassz aktív járművet.";
        }

        if (!isValidDate(form.date)) {
            nextErrors.date =
                "Adj meg érvényes dátumot: ÉÉÉÉ-HH-NN.";
        }

        const odometerKm = parseNonNegative(form.odometerKm);
        const liters = parseNonNegative(form.liters);
        const totalPrice = parseNonNegative(form.totalPrice);

        if (odometerKm === null) {
            nextErrors.odometerKm =
                "Adj meg egy nem negatív kilométeróra-állást.";
        }

        if (liters === null || liters <= 0) {
            nextErrors.liters =
                "A tankolt mennyiség legyen nullánál nagyobb.";
        }

        if (totalPrice === null || totalPrice <= 0) {
            nextErrors.totalPrice =
                "A teljes ár legyen nullánál nagyobb.";
        }

        setErrors(nextErrors);

        if (
            Object.keys(nextErrors).length > 0 ||
            odometerKm === null ||
            liters === null ||
            totalPrice === null
        ) {
            return null;
        }

        return {
            vehicleId: form.vehicleId,
            date: form.date,
            odometerKm,
            liters,
            totalPrice,
            isFullTank: form.isFullTank,
            note: form.note.trim() || null,
        };
    }

    async function handleSubmit() {
        if (submittingRef.current) {
            return;
        }

        setFormError(null);
        setSuccessMessage(null);

        const request = buildRequest();

        if (!request) {
            return;
        }

        submittingRef.current = true;
        setIsSubmitting(true);

        try {
            await createFuelFillUp(request);

            setForm({
                ...initialValues(),
                vehicleId: request.vehicleId,
                date: request.date,
            });

            setErrors({});

            setSuccessMessage(
                "A tankolást elmentettük. Az Áttekintés fülön ellenőrizheted az összesítést."
            );
        } catch (error) {
            if (error instanceof ApiError) {
                const fieldErrors: FieldErrors = {};

                for (const field of Object.keys(form) as FieldName[]) {
                    const message = error.fieldError(field);

                    if (message) {
                        fieldErrors[field] = message;
                    }
                }

                setErrors(fieldErrors);

                setFormError(
                    Object.keys(fieldErrors).length > 0
                        ? "Ellenőrizd a jelölt mezőket."
                        : error.message
                );
            } else {
                setFormError(
                    "Nem sikerült elmenteni a tankolást. Próbáld újra később."
                );
            }
        } finally {
            submittingRef.current = false;
            setIsSubmitting(false);
        }
    }

    if (isLoadingVehicles) {
        return (
            <FuelFillUpFormSkeleton
                vehicleCount={vehicles.length || 1}
            />
        );
    }

    if (vehiclesError) {
        return (
            <View style={styles.form}>
                <FeedbackMessage message={vehiclesError} />

                <PrimaryButton
                    title="Újrapróbálom"
                    onPress={() =>
                        setReloadKey((value) => value + 1)
                    }
                />
            </View>
        );
    }

    if (vehicles.length === 0) {
        return (
            <FeedbackMessage
                message="Tankolás rögzítéséhez legalább egy aktív jármű szükséges."
            />
        );
    }

    return (
        <View style={styles.form}>
            <ChoiceField
                label="Melyik járművet tankoltad?"
                options={vehicles.map((vehicle) => ({
                    value: vehicle.id,
                    label: vehicle.name,
                }))}
                value={form.vehicleId}
                onChange={(value) =>
                    updateField("vehicleId", value)
                }
                disabled={isSubmitting}
                error={errors.vehicleId}
            />

            <DateField
                label="Tankolás dátuma"
                value={form.date}
                onChange={(value) => updateField("date", value)}
                disabled={isSubmitting}
                error={errors.date}
            />

            <FormField
                label="Kilométeróra-állás (km)"
                placeholder="Például: 125430"
                keyboardType="decimal-pad"
                value={form.odometerKm}
                onChangeText={(value) =>
                    updateField("odometerKm", value)
                }
                editable={!isSubmitting}
                error={errors.odometerKm}
            />

            <FormField
                label="Tankolt mennyiség (liter)"
                placeholder="Például: 35,5"
                keyboardType="decimal-pad"
                value={form.liters}
                onChangeText={(value) =>
                    updateField("liters", value)
                }
                editable={!isSubmitting}
                error={errors.liters}
            />

            <FormField
                label="Teljes ár (Ft)"
                placeholder="Például: 22000"
                keyboardType="decimal-pad"
                value={form.totalPrice}
                onChangeText={(value) =>
                    updateField("totalPrice", value)
                }
                editable={!isSubmitting}
                error={errors.totalPrice}
            />

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
                        Tele tank
                    </Text>

                    <Text
                        style={[
                            styles.help,
                            { color: colors.textSecondary },
                        ]}
                    >
                        Kapcsold be, ha teljesen feltöltötted a tankot.
                    </Text>
                </View>

                <Switch
                    accessibilityLabel="Tele tank"
                    disabled={isSubmitting}
                    value={form.isFullTank}
                    onValueChange={(value) =>
                        updateField("isFullTank", value)
                    }
                    trackColor={{
                        false: colors.switchTrackOff,
                        true: colors.switchTrackOn,
                    }}
                    thumbColor={colors.switchThumb}
                />
            </View>

            {errors.isFullTank ? (
                <Text
                    style={[
                        styles.fieldError,
                        { color: colors.danger },
                    ]}
                >
                    {errors.isFullTank}
                </Text>
            ) : null}

            <FormField
                label="Megjegyzés"
                placeholder="Nem kötelező"
                multiline
                value={form.note}
                onChangeText={(value) =>
                    updateField("note", value)
                }
                editable={!isSubmitting}
                error={errors.note}
            />

            <Text
                style={[
                    styles.help,
                    { color: colors.textSecondary },
                ]}
            >
                A tankolás árát ne rögzítsd külön kiadásként vagy
                műszakkiadásként is, mert a jelenlegi dashboard
                összeadja ezeket.
            </Text>

            {formError ? (
                <FeedbackMessage message={formError} />
            ) : null}

            {successMessage ? (
                <FeedbackMessage
                    message={successMessage}
                    variant="success"
                />
            ) : null}

            <PrimaryButton
                title="Tankolás mentése"
                loading={isSubmitting}
                onPress={() => void handleSubmit()}
            />
        </View>
    );
}

const styles = StyleSheet.create({
    form: {
        gap: 18,
    },
    help: {
        fontSize: 13,
        lineHeight: 19,
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
    fieldError: {
        fontSize: 13,
        lineHeight: 18,
    },
});