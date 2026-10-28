import {
    useCallback,
    useEffect,
    useRef,
    useState,
} from "react";
import {
    StyleSheet,
    Text,
    View,
} from "react-native";
import { useFocusEffect } from "@react-navigation/native";

import { ApiError } from "../../api/client";
import {
    createVehicle,
    getVehicles,
    updateVehicle,
} from "../../api/endpoints";
import { useTheme } from "../../theme/ThemeProvider";
import type {
    UpdateVehicleRequest,
    VehicleResponse,
} from "../../types/api";

import { FadeInView } from "../ui/FadeInView";
import { FeedbackMessage } from "../ui/FeedbackMessage";
import { PrimaryButton } from "../ui/PrimaryButton";
import { VehicleCard } from "./VehicleCard";
import { VehicleForm } from "./VehicleForm";
import { UserVehiclesSkeleton } from "./UserVehiclesSkeleton";

type SuccessFeedback = {
    target: "create" | "edit";
    vehicleId?: string;
    message: string;
};

function sortVehicles(
    vehicles: VehicleResponse[]
): VehicleResponse[] {
    return [...vehicles].sort(
        (a, b) =>
            Number(b.isActive) - Number(a.isActive) ||
            a.name.localeCompare(b.name, "hu")
    );
}

export function UserVehiclesSection() {
    const { colors } = useTheme();

    const mountedRef = useRef(false);
    const submittingRef = useRef(false);
    const requestVersionRef = useRef(0);
    const hasLoadedRef = useRef(false);

    const [vehicles, setVehicles] =
        useState<VehicleResponse[]>([]);

    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [loadError, setLoadError] =
        useState<string | null>(null);

    const [reloadKey, setReloadKey] = useState(0);

    const [editingId, setEditingId] =
        useState<string | null>(null);

    const [success, setSuccess] =
        useState<SuccessFeedback | null>(null);

    const [createFormKey, setCreateFormKey] = useState(0);

    useEffect(() => {
        mountedRef.current = true;

        return () => {
            mountedRef.current = false;
        };
    }, []);

    useFocusEffect(
        useCallback(() => {
            let active = true;
            const requestVersion = ++requestVersionRef.current;

            setIsLoading(true);
            setLoadError(null);

            async function loadVehicles() {
                try {
                    const data = await getVehicles();

                    if (
                        !active ||
                        requestVersion !== requestVersionRef.current
                    ) {
                        return;
                    }

                    setVehicles(sortVehicles(data));
                    hasLoadedRef.current = true;

                    setEditingId((current) =>
                        current &&
                            data.some((vehicle) => vehicle.id === current)
                            ? current
                            : null
                    );
                } catch (error) {
                    if (
                        !active ||
                        requestVersion !== requestVersionRef.current
                    ) {
                        return;
                    }

                    setLoadError(
                        error instanceof ApiError
                            ? error.message
                            : "Nem sikerült betölteni a járműveket."
                    );
                } finally {
                    if (
                        active &&
                        requestVersion === requestVersionRef.current
                    ) {
                        setIsLoading(false);
                    }
                }
            }

            void loadVehicles();

            return () => {
                active = false;
            };
        }, [reloadKey])
    );

    async function saveVehicle(
        request: UpdateVehicleRequest,
        vehicleId?: string
    ): Promise<void> {
        if (
            submittingRef.current ||
            isLoading ||
            loadError ||
            !mountedRef.current
        ) {
            throw new Error("A mentés jelenleg nem indítható.");
        }

        submittingRef.current = true;
        setIsSubmitting(true);
        setSuccess(null);

        try {
            const saved = vehicleId
                ? await updateVehicle(vehicleId, request)
                : await createVehicle({
                    name: request.name,
                    type: request.type,
                    startingOdometerKm: request.startingOdometerKm,
                });

            if (!mountedRef.current) {
                return;
            }

            requestVersionRef.current += 1;
            hasLoadedRef.current = true;

            setIsLoading(false);
            setLoadError(null);

            setVehicles((current) =>
                sortVehicles([
                    ...current.filter(
                        (vehicle) => vehicle.id !== saved.id
                    ),
                    saved,
                ])
            );

            if (vehicleId) {
                setEditingId(null);

                setSuccess({
                    target: "edit",
                    vehicleId: saved.id,
                    message: "A jármű módosításait elmentettük.",
                });
            } else {
                setCreateFormKey((current) => current + 1);

                setSuccess({
                    target: "create",
                    message:
                        "A járművet hozzáadtuk. Új műszak és tankolás rögzítésekor kiválaszthatod.",
                });
            }
        } finally {
            submittingRef.current = false;

            if (mountedRef.current) {
                setIsSubmitting(false);
            }
        }
    }

    function startEditing(id: string) {
        if (submittingRef.current || isLoading) {
            return;
        }

        setSuccess(null);
        setEditingId(id);
    }

    function cancelEditing() {
        if (submittingRef.current) {
            return;
        }

        setEditingId(null);
        setSuccess(null);
    }

    if (isLoading) {
        return (
            <UserVehiclesSkeleton
                itemCount={
                    hasLoadedRef.current
                        ? vehicles.length
                        : 2
                }
            />
        );
    }

    if (loadError) {
        return (
            <View style={styles.section}>
                <Text
                    style={[
                        styles.heading,
                        { color: colors.textPrimary },
                    ]}
                >
                    Saját járműveim
                </Text>

                <FeedbackMessage message={loadError} />

                <PrimaryButton
                    title="Újrapróbálom"
                    onPress={() =>
                        setReloadKey((current) => current + 1)
                    }
                />
            </View>
        );
    }

    return (
        <FadeInView
            translateY={0}
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

            {vehicles.length === 0 ? (
                <Text
                    style={[
                        styles.help,
                        { color: colors.textSecondary },
                    ]}
                >
                    Még nem adtál hozzá járművet. Az alábbi űrlappal
                    felveheted az elsőt.
                </Text>
            ) : (
                vehicles.map((vehicle) => {
                    const isEditing = editingId === vehicle.id;

                    return (
                        <View
                            key={vehicle.id}
                            style={styles.vehicleItem}
                        >
                            <VehicleCard
                                vehicle={vehicle}
                                isEditing={isEditing}
                                disabled={isSubmitting}
                                onEdit={() => startEditing(vehicle.id)}
                                onCancel={cancelEditing}
                            >
                                {isEditing ? (
                                    <VehicleForm
                                        key={vehicle.id}
                                        vehicle={vehicle}
                                        disabled={isSubmitting}
                                        onSave={(request) =>
                                            saveVehicle(request, vehicle.id)
                                        }
                                        onCancel={cancelEditing}
                                    />
                                ) : null}
                            </VehicleCard>

                            {success?.target === "edit" &&
                                success.vehicleId === vehicle.id ? (
                                <FeedbackMessage
                                    message={success.message}
                                    variant="success"
                                />
                            ) : null}
                        </View>
                    );
                })
            )}

            {!editingId ? (
                <View
                    style={[
                        styles.createSection,
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

                    <VehicleForm
                        key={createFormKey}
                        disabled={isSubmitting}
                        onSave={(request) => saveVehicle(request)}
                    />

                    {success?.target === "create" ? (
                        <FeedbackMessage
                            message={success.message}
                            variant="success"
                        />
                    ) : null}
                </View>
            ) : null}
        </FadeInView>
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
    help: {
        fontSize: 13,
        lineHeight: 19,
    },
    vehicleItem: {
        gap: 10,
    },
    createSection: {
        borderTopWidth: 1,
        gap: 14,
        paddingTop: 20,
    },
});