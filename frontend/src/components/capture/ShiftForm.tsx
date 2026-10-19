import { useCallback, useRef, useState } from "react";
import {
    ActivityIndicator,
    StyleSheet,
    Text,
    View,
} from "react-native";
import { useFocusEffect } from "@react-navigation/native";
import { ApiError } from "../../api/client";
import {
    createShift,
    getVehicles,
} from "../../api/endpoints";
import {
    getUserCompanies,
} from "../../api/userCompanies";
import type {
    UserCompanyResponse,
} from "../../api/userCompanies";
import type {
    CreateShiftRequest,
    VehicleResponse,
} from "../../types/api";
import { FormField } from "../ui/FormField";
import { PrimaryButton } from "../ui/PrimaryButton";
import { FeedbackMessage } from "../ui/FeedbackMessage";
import { ChoiceField } from "../ui/ChoiceField";
import { DateField } from "../ui/DateField";

type FormValues = {
    userCompanyId: string;
    vehicleId: string;
    date: string;
    workedHours: string;
    workedMinutes: string;
    drivenKilometers: string;
    revenue: string;
    deliveredOrders: string;
    workExpense: string;
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
        userCompanyId: "",
        vehicleId: "",
        date: todayLocal(),
        workedHours: "",
        workedMinutes: "",
        drivenKilometers: "",
        revenue: "",
        deliveredOrders: "",
        workExpense: "",
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

function parseNonNegative(
    value: string,
    integer = false
): number | null {
    const normalized = value.trim().replace(",", ".");
    const pattern = integer ? /^\d+$/ : /^\d+(?:\.\d+)?$/;

    if (!pattern.test(normalized)) {
        return null;
    }

    const parsed = Number(normalized);

    if (
        !Number.isFinite(parsed) ||
        parsed < 0 ||
        (integer && !Number.isSafeInteger(parsed))
    ) {
        return null;
    }

    return parsed;
}

export function ShiftForm() {
    const submittingRef = useRef(false);

    const [companies, setCompanies] =
        useState<UserCompanyResponse[]>([]);
    const [vehicles, setVehicles] =
        useState<VehicleResponse[]>([]);

    const [isLoadingOptions, setIsLoadingOptions] = useState(true);
    const [optionsError, setOptionsError] =
        useState<string | null>(null);
    const [reloadKey, setReloadKey] = useState(0);

    const [form, setForm] = useState<FormValues>(initialValues);
    const [errors, setErrors] = useState<FieldErrors>({});
    const [formError, setFormError] = useState<string | null>(null);
    const [successMessage, setSuccessMessage] =
        useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useFocusEffect(
        useCallback(() => {
            let active = true;

            setIsLoadingOptions(true);
            setOptionsError(null);

            async function loadOptions() {
                try {
                    const [companyData, vehicleData] = await Promise.all([
                        getUserCompanies(),
                        getVehicles(),
                    ]);

                    if (!active) {
                        return;
                    }

                    setCompanies(companyData);
                    setVehicles(
                        vehicleData.filter((vehicle) => vehicle.isActive)
                    );
                } catch (error) {
                    if (!active) {
                        return;
                    }

                    setOptionsError(
                        error instanceof ApiError
                            ? error.message
                            : "Nem sikerült betölteni a cégeket és járműveket."
                    );
                } finally {
                    if (active) {
                        setIsLoadingOptions(false);
                    }
                }
            }

            void loadOptions();

            return () => {
                active = false;
            };
        }, [reloadKey])
    );

    function updateField(field: FieldName, value: string) {
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

    function buildRequest(): CreateShiftRequest | null {
        const nextErrors: FieldErrors = {};

        const company = companies.find(
            (item) => item.id === form.userCompanyId
        );

        if (!company) {
            nextErrors.userCompanyId = "Válassz céget.";
        }

        if (!vehicles.some((item) => item.id === form.vehicleId)) {
            nextErrors.vehicleId = "Válassz aktív járművet.";
        }

        const validDate = isValidDate(form.date);

        if (!validDate) {
            nextErrors.date =
                "Adj meg érvényes dátumot: ÉÉÉÉ-HH-NN.";
        }

        if (company && validDate) {
            if (
                form.date < company.startedOn ||
                (company.endedOn !== null &&
                    form.date > company.endedOn)
            ) {
                nextErrors.date =
                    "A dátum essen a kiválasztott céges kapcsolat időszakába.";
            }
        }

        const workedHours = parseNonNegative(
            form.workedHours,
            true
        );
        const workedMinutes = parseNonNegative(
            form.workedMinutes,
            true
        );
        const drivenKilometers = parseNonNegative(
            form.drivenKilometers
        );
        const revenue = parseNonNegative(form.revenue);
        const deliveredOrders = parseNonNegative(
            form.deliveredOrders,
            true
        );
        const workExpense = parseNonNegative(form.workExpense);

        if (workedHours === null) {
            nextErrors.workedHours =
                "Adj meg egy nem negatív egész óraszámot.";
        }

        if (workedMinutes === null || workedMinutes > 59) {
            nextErrors.workedMinutes =
                "A perc 0 és 59 közötti egész szám legyen.";
        }

        if (drivenKilometers === null) {
            nextErrors.drivenKilometers =
                "Adj meg egy nem negatív kilométerértéket.";
        }

        if (revenue === null) {
            nextErrors.revenue =
                "Adj meg egy nem negatív bevételt.";
        }

        if (deliveredOrders === null) {
            nextErrors.deliveredOrders =
                "Adj meg egy nem negatív egész darabszámot.";
        }

        if (workExpense === null) {
            nextErrors.workExpense =
                "Adj meg egy nem negatív kiadást.";
        }

        setErrors(nextErrors);

        if (
            Object.keys(nextErrors).length > 0 ||
            workedHours === null ||
            workedMinutes === null ||
            drivenKilometers === null ||
            revenue === null ||
            deliveredOrders === null ||
            workExpense === null
        ) {
            return null;
        }

        return {
            userCompanyId: form.userCompanyId,
            vehicleId: form.vehicleId,
            date: form.date,
            workedHours,
            workedMinutes,
            drivenKilometers,
            revenue,
            deliveredOrders,
            workExpense,
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
            await createShift(request);

            setForm({
                ...initialValues(),
                userCompanyId: request.userCompanyId,
                vehicleId: request.vehicleId,
                date: request.date,
            });

            setErrors({});
            setSuccessMessage(
                "A műszakot elmentettük. Az Áttekintés fülön ellenőrizheted az összesítést."
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
                    "Nem sikerült elmenteni a műszakot. Próbáld újra később."
                );
            }
        } finally {
            submittingRef.current = false;
            setIsSubmitting(false);
        }
    }

    if (isLoadingOptions) {
        return (
            <View style={styles.loading}>
                <ActivityIndicator color="#365CF5" size="large" />
                <Text style={styles.help}>Cégek és járművek betöltése…</Text>
            </View>
        );
    }

    if (optionsError) {
        return (
            <View style={styles.form}>
                <FeedbackMessage message={optionsError} />

                <PrimaryButton
                    title="Újrapróbálom"
                    onPress={() => setReloadKey((value) => value + 1)}
                />
            </View>
        );
    }

    if (companies.length === 0 || vehicles.length === 0) {
        return (
            <FeedbackMessage
                message={
                    companies.length === 0
                        ? "Műszak rögzítéséhez előbb céges kapcsolatot kell hozzáadnod. Jelenleg nincs elérhető saját céged."
                        : "Műszak rögzítéséhez legalább egy aktív jármű szükséges."
                }
            />
        );
    }

    return (
        <View style={styles.form}>
            <ChoiceField
                label="Melyik cégnek dolgoztál?"
                options={companies.map((company) => ({
                    value: company.id,
                    label: company.deliveryCompanyName,
                    description:
                        `${company.startedOn} – ${company.endedOn ?? "nincs záródátum"}` +
                        (company.isActive ? "" : " · Inaktív kapcsolat"),
                }))}
                value={form.userCompanyId}
                onChange={(value) => updateField("userCompanyId", value)}
                disabled={isSubmitting}
                error={errors.userCompanyId}
            />

            <ChoiceField
                label="Melyik járművel?"
                options={vehicles.map((vehicle) => ({
                    value: vehicle.id,
                    label: vehicle.name,
                }))}
                value={form.vehicleId}
                onChange={(value) => updateField("vehicleId", value)}
                disabled={isSubmitting}
                error={errors.vehicleId}
            />

            <DateField
                label="Műszak dátuma"
                value={form.date}
                onChange={(value) => updateField("date", value)}
                disabled={isSubmitting}
                error={errors.date}
            />

            <View style={styles.row}>
                <View style={styles.column}>
                    <FormField
                        label="Munkaidő – óra"
                        placeholder="Például: 5"
                        keyboardType="number-pad"
                        value={form.workedHours}
                        onChangeText={(value) =>
                            updateField("workedHours", value)
                        }
                        editable={!isSubmitting}
                        error={errors.workedHours}
                    />
                </View>

                <View style={styles.column}>
                    <FormField
                        label="Munkaidő – perc"
                        placeholder="Például: 28"
                        keyboardType="number-pad"
                        value={form.workedMinutes}
                        onChangeText={(value) =>
                            updateField("workedMinutes", value)
                        }
                        editable={!isSubmitting}
                        error={errors.workedMinutes}
                    />
                </View>
            </View>

            <FormField
                label="Bevétel (Ft)"
                placeholder="Például: 28934"
                keyboardType="decimal-pad"
                value={form.revenue}
                onChangeText={(value) => updateField("revenue", value)}
                editable={!isSubmitting}
                error={errors.revenue}
            />

            <FormField
                label="Kiszállítások száma"
                placeholder="Például: 13"
                keyboardType="number-pad"
                value={form.deliveredOrders}
                onChangeText={(value) =>
                    updateField("deliveredOrders", value)
                }
                editable={!isSubmitting}
                error={errors.deliveredOrders}
            />

            <FormField
                label="Megtett út (km)"
                placeholder="Például: 120 vagy 120,5"
                keyboardType="decimal-pad"
                value={form.drivenKilometers}
                onChangeText={(value) =>
                    updateField("drivenKilometers", value)
                }
                editable={!isSubmitting}
                error={errors.drivenKilometers}
            />

            <FormField
                label="Műszakhoz tartozó kiadás (Ft)"
                placeholder="Például: 1000"
                keyboardType="decimal-pad"
                value={form.workExpense}
                onChangeText={(value) =>
                    updateField("workExpense", value)
                }
                editable={!isSubmitting}
                error={errors.workExpense}
            />

            <Text style={styles.help}>
                Ugyanazt a kiadást ne rögzítsd itt és külön kiadásként is,
                mert a jelenlegi dashboard mindkettőt összeadja.
            </Text>

            <FormField
                label="Megjegyzés"
                placeholder="Nem kötelező"
                multiline
                value={form.note}
                onChangeText={(value) => updateField("note", value)}
                editable={!isSubmitting}
                error={errors.note}
            />

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
                title="Műszak mentése"
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
    row: {
        flexDirection: "row",
        gap: 12,
    },
    column: {
        flex: 1,
        minWidth: 0,
    },
    loading: {
        alignItems: "center",
        gap: 12,
        paddingVertical: 36,
    },
    help: {
        color: "#666B7A",
        fontSize: 13,
        lineHeight: 19,
    },
});