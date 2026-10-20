import {
    useCallback,
    useEffect,
    useMemo,
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
import { useFocusEffect } from "@react-navigation/native";

import { ApiError } from "../../api/client";
import {
    createDeliveryCompany,
    createUserCompany,
    getDeliveryCompanies,
    getUserCompanies,
    updateUserCompany,
} from "../../api/endpoints";
import type {
    DeliveryCompanyResponse,
    UserCompanyResponse,
} from "../../types/api";

import { useTheme } from "../../theme/ThemeProvider";
import type { ThemeColors } from "../../theme/themes";

import { ChoiceField } from "../ui/ChoiceField";
import { DateField } from "../ui/DateField";
import { FadeInView } from "../ui/FadeInView";
import { FeedbackMessage } from "../ui/FeedbackMessage";
import { FormField } from "../ui/FormField";
import { PrimaryButton } from "../ui/PrimaryButton";
import { UserCompaniesSkeleton } from "./UserCompaniesSkeleton";

type FeedbackTarget = "create" | "add" | "edit";

function todayLocal(): string {
    const now = new Date();

    return [
        now.getFullYear(),
        String(now.getMonth() + 1).padStart(2, "0"),
        String(now.getDate()).padStart(2, "0"),
    ].join("-");
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

function errorMessage(error: unknown): string {
    return error instanceof ApiError
        ? error.message
        : "Nem sikerült végrehajtani a műveletet. Próbáld újra.";
}

function sortCompanies(
    companies: DeliveryCompanyResponse[]
): DeliveryCompanyResponse[] {
    return [...companies].sort((a, b) =>
        a.name.localeCompare(b.name, "hu")
    );
}

function sortUserCompanies(
    companies: UserCompanyResponse[]
): UserCompanyResponse[] {
    return [...companies].sort(
        (a, b) =>
            Number(b.isActive) - Number(a.isActive) ||
            a.deliveryCompanyName.localeCompare(
                b.deliveryCompanyName,
                "hu"
            )
    );
}

export function UserCompaniesSection() {
    const { colors } = useTheme();

    const styles = useMemo(
        () => createStyles(colors),
        [colors]
    );

    const mountedRef = useRef(false);
    const submittingRef = useRef(false);
    const mutationVersionRef = useRef(0);

    const [companies, setCompanies] =
        useState<DeliveryCompanyResponse[]>([]);

    const [userCompanies, setUserCompanies] =
        useState<UserCompanyResponse[]>([]);

    const [companyId, setCompanyId] = useState("");
    const [startedOn, setStartedOn] = useState(todayLocal);
    const [note, setNote] = useState("");
    const [newCompanyName, setNewCompanyName] = useState("");

    const [editingId, setEditingId] =
        useState<string | null>(null);

    const [editStartedOn, setEditStartedOn] = useState("");
    const [editEndedOn, setEditEndedOn] = useState("");
    const [editIsActive, setEditIsActive] = useState(true);
    const [editNote, setEditNote] = useState("");

    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [reloadKey, setReloadKey] = useState(0);

    const [loadError, setLoadError] =
        useState<string | null>(null);

    const [actionError, setActionError] =
        useState<string | null>(null);

    const [success, setSuccess] =
        useState<string | null>(null);

    const [feedbackTarget, setFeedbackTarget] =
        useState<FeedbackTarget | null>(null);

    const [lastEditedId, setLastEditedId] =
        useState<string | null>(null);

    useEffect(() => {
        mountedRef.current = true;

        return () => {
            mountedRef.current = false;
        };
    }, []);

    useFocusEffect(
        useCallback(() => {
            let active = true;
            const versionAtStart = mutationVersionRef.current;

            setIsLoading(true);
            setLoadError(null);

            async function load() {
                try {
                    const [companyList, ownCompanyList] =
                        await Promise.all([
                            getDeliveryCompanies(),
                            getUserCompanies(),
                        ]);

                    if (
                        !active ||
                        versionAtStart !== mutationVersionRef.current
                    ) {
                        return;
                    }

                    setCompanies(sortCompanies(companyList));
                    setUserCompanies(sortUserCompanies(ownCompanyList));

                    setCompanyId((current) => {
                        const isAvailable =
                            companyList.some(
                                (company) => company.id === current
                            ) &&
                            !ownCompanyList.some(
                                (company) =>
                                    company.deliveryCompanyId === current
                            );

                        return isAvailable ? current : "";
                    });

                    setEditingId((current) =>
                        current &&
                            ownCompanyList.some(
                                (company) => company.id === current
                            )
                            ? current
                            : null
                    );
                } catch (error) {
                    if (
                        active &&
                        versionAtStart === mutationVersionRef.current
                    ) {
                        setLoadError(errorMessage(error));
                    }
                } finally {
                    if (active) {
                        setIsLoading(false);
                    }
                }
            }

            void load();

            return () => {
                active = false;
            };
        }, [reloadKey])
    );

    const availableCompanies = companies.filter(
        (company) =>
            !userCompanies.some(
                (ownCompany) =>
                    ownCompany.deliveryCompanyId === company.id
            )
    );

    const disabled = isLoading || isSubmitting;

    function clearFeedback() {
        setActionError(null);
        setSuccess(null);
        setFeedbackTarget(null);
        setLastEditedId(null);
    }

    async function runAction(
        target: FeedbackTarget,
        action: () => Promise<void>
    ) {
        if (
            submittingRef.current ||
            isLoading ||
            loadError ||
            !mountedRef.current
        ) {
            return;
        }

        submittingRef.current = true;
        setIsSubmitting(true);
        clearFeedback();
        setFeedbackTarget(target);

        try {
            await action();
        } catch (error) {
            if (mountedRef.current) {
                setActionError(errorMessage(error));
            }
        } finally {
            submittingRef.current = false;

            if (mountedRef.current) {
                setIsSubmitting(false);
            }
        }
    }

    async function handleAddCompany() {
        await runAction("add", async () => {
            const selectedCompany = availableCompanies.find(
                (company) => company.id === companyId
            );

            if (!selectedCompany) {
                setActionError(
                    "Válassz egy még hozzá nem rendelt futárcéget."
                );
                return;
            }

            if (!isValidDate(startedOn)) {
                setActionError("Adj meg érvényes kezdési dátumot.");
                return;
            }

            if (note.trim().length > 500) {
                setActionError(
                    "A megjegyzés legfeljebb 500 karakter lehet."
                );
                return;
            }

            const created = await createUserCompany({
                deliveryCompanyId: selectedCompany.id,
                startedOn,
                note: note.trim() || null,
            });

            if (!mountedRef.current) {
                return;
            }

            mutationVersionRef.current += 1;

            setUserCompanies((current) =>
                sortUserCompanies([
                    ...current.filter(
                        (company) => company.id !== created.id
                    ),
                    created,
                ])
            );

            setCompanyId("");
            setNote("");

            setSuccess(
                "A futárcéget hozzáadtuk a saját cégeidhez."
            );
        });
    }

    async function handleCreateCompany() {
        await runAction("create", async () => {
            const name = newCompanyName.trim();

            if (!name || name.length > 100) {
                setActionError(
                    "Adj meg egy 1–100 karakteres cégnevet."
                );
                return;
            }

            const normalizedName = name.toLocaleLowerCase("hu-HU");

            const existingCompany = companies.find(
                (company) =>
                    company.name.trim().toLocaleLowerCase("hu-HU") ===
                    normalizedName
            );

            if (existingCompany) {
                const alreadyAssigned = userCompanies.some(
                    (company) =>
                        company.deliveryCompanyId === existingCompany.id
                );

                if (alreadyAssigned) {
                    setActionError(
                        "Ez a cég már szerepel a saját cégeid között. " +
                        "Inaktív kapcsolatot a Szerkesztés gombbal " +
                        "aktiválhatsz újra."
                    );
                } else {
                    setCompanyId(existingCompany.id);

                    setActionError(
                        "Ez a futárcég már létezik. " +
                        "Kiválasztottuk a fenti hozzáadási űrlapon."
                    );
                }

                return;
            }

            const created = await createDeliveryCompany({ name });

            if (!mountedRef.current) {
                return;
            }

            mutationVersionRef.current += 1;

            setCompanies((current) =>
                sortCompanies([
                    ...current.filter(
                        (company) => company.id !== created.id
                    ),
                    created,
                ])
            );

            setCompanyId(created.id);
            setNewCompanyName("");

            setSuccess(
                "A futárcég létrejött. A fenti űrlapon már " +
                "ki van választva: add meg a kezdést, majd nyomd meg " +
                "a „Hozzáadás a saját cégeimhez” gombot."
            );
        });
    }

    function startEditing(company: UserCompanyResponse) {
        if (disabled || submittingRef.current) {
            return;
        }

        setEditingId(company.id);
        setEditStartedOn(company.startedOn);
        setEditEndedOn(company.endedOn ?? "");
        setEditIsActive(company.isActive);
        setEditNote(company.note ?? "");
        clearFeedback();
    }

    function cancelEditing() {
        if (submittingRef.current) {
            return;
        }

        setEditingId(null);
        clearFeedback();
    }

    async function handleUpdateCompany() {
        const id = editingId;

        if (!id) {
            return;
        }

        await runAction("edit", async () => {
            if (!isValidDate(editStartedOn)) {
                setActionError("Adj meg érvényes kezdési dátumot.");
                return;
            }

            if (editEndedOn && !isValidDate(editEndedOn)) {
                setActionError(
                    "Adj meg érvényes befejezési dátumot."
                );
                return;
            }

            if (editEndedOn && editEndedOn < editStartedOn) {
                setActionError(
                    "A befejezés nem lehet korábbi a kezdésnél."
                );
                return;
            }

            if (editIsActive && editEndedOn) {
                setActionError(
                    "Aktív kapcsolatnál töröld a befejezési dátumot."
                );
                return;
            }

            if (editNote.trim().length > 500) {
                setActionError(
                    "A megjegyzés legfeljebb 500 karakter lehet."
                );
                return;
            }

            const updated = await updateUserCompany(id, {
                startedOn: editStartedOn,
                endedOn: editEndedOn || null,
                isActive: editIsActive,
                note: editNote.trim() || null,
            });

            if (!mountedRef.current) {
                return;
            }

            mutationVersionRef.current += 1;

            setUserCompanies((current) =>
                sortUserCompanies(
                    current.map((company) =>
                        company.id === updated.id ? updated : company
                    )
                )
            );

            setLastEditedId(updated.id);
            setEditingId(null);

            setSuccess(
                "A saját cégkapcsolat módosításait elmentettük."
            );
        });
    }

    function renderFeedback(target: FeedbackTarget) {
        if (
            feedbackTarget !== target ||
            (!actionError && !success)
        ) {
            return null;
        }

        return (
            <View style={styles.feedback}>
                {actionError ? (
                    <FeedbackMessage message={actionError} />
                ) : null}

                {success ? (
                    <FeedbackMessage
                        message={success}
                        variant="success"
                    />
                ) : null}
            </View>
        );
    }

    if (isLoading) {
        return (
            <UserCompaniesSkeleton
                companyCount={userCompanies.length || 2}
                availableCompanyCount={availableCompanies.length || 2}
            />
        );
    }

    if (loadError) {
        return (
            <View style={styles.section}>
                <Text style={styles.heading}>
                    Saját futárcégeim
                </Text>

                <FeedbackMessage message={loadError} />

                <PrimaryButton
                    title="Újrapróbálom"
                    onPress={() =>
                        setReloadKey((value) => value + 1)
                    }
                />
            </View>
        );
    }

    return (
        <FadeInView style={styles.section}>
            <Text style={styles.heading}>
                Saját futárcégeim
            </Text>

            {userCompanies.length === 0 ? (
                <Text style={styles.help}>
                    Még nem adtál hozzá futárcéget a profilodhoz.
                </Text>
            ) : (
                userCompanies.map((company) => {
                    const isEditing = editingId === company.id;

                    return (
                        <View
                            key={company.id}
                            style={[
                                styles.card,
                                isEditing ? styles.editingCard : null,
                            ]}
                        >
                            <View style={styles.cardHeader}>
                                <Text style={styles.companyName}>
                                    {company.deliveryCompanyName}
                                </Text>

                                <View
                                    style={[
                                        styles.statusBadge,
                                        company.isActive
                                            ? styles.activeBadge
                                            : styles.inactiveBadge,
                                    ]}
                                >
                                    <Text
                                        style={[
                                            styles.statusText,
                                            company.isActive
                                                ? styles.activeText
                                                : styles.inactiveText,
                                        ]}
                                    >
                                        {company.isActive ? "Aktív" : "Inaktív"}
                                    </Text>
                                </View>
                            </View>

                            <Pressable
                                accessibilityRole="button"
                                accessibilityLabel={
                                    `${company.deliveryCompanyName} ` +
                                    (isEditing
                                        ? "szerkesztésének bezárása"
                                        : "kapcsolatának szerkesztése")
                                }
                                accessibilityState={{
                                    disabled,
                                    expanded: isEditing,
                                }}
                                disabled={disabled}
                                onPress={() => {
                                    if (isEditing) {
                                        cancelEditing();
                                    } else {
                                        startEditing(company);
                                    }
                                }}
                                style={({ pressed }) => [
                                    styles.editButton,
                                    pressed && !disabled
                                        ? styles.buttonPressed
                                        : null,
                                    disabled ? styles.buttonDisabled : null,
                                ]}
                            >
                                <Text style={styles.editButtonText}>
                                    {isEditing
                                        ? "Szerkesztés bezárása"
                                        : "Szerkesztés"}
                                </Text>
                            </Pressable>

                            {isEditing ? (
                                <View style={styles.inlineEditForm}>
                                    <DateField
                                        label="Kezdés dátuma"
                                        value={editStartedOn}
                                        onChange={(value) => {
                                            setEditStartedOn(value);
                                            clearFeedback();
                                        }}
                                        disabled={disabled}
                                    />

                                    <View style={styles.switchRow}>
                                        <Text style={styles.switchLabel}>
                                            Aktív kapcsolat
                                        </Text>

                                        <Switch
                                            accessibilityLabel="Aktív cégkapcsolat"
                                            value={editIsActive}
                                            disabled={disabled}
                                            onValueChange={(value) => {
                                                setEditIsActive(value);

                                                if (value) {
                                                    setEditEndedOn("");
                                                }

                                                clearFeedback();
                                            }}
                                            trackColor={{
                                                false: colors.switchTrackOff,
                                                true: colors.switchTrackOn,
                                            }}
                                            thumbColor={colors.switchThumb}
                                        />
                                    </View>

                                    {!editIsActive ? (
                                        <>
                                            <DateField
                                                label="Befejezés dátuma – nem kötelező"
                                                value={editEndedOn}
                                                onChange={(value) => {
                                                    setEditEndedOn(value);
                                                    clearFeedback();
                                                }}
                                                disabled={disabled}
                                            />

                                            {editEndedOn ? (
                                                <Pressable
                                                    accessibilityRole="button"
                                                    accessibilityState={{ disabled }}
                                                    disabled={disabled}
                                                    onPress={() => {
                                                        setEditEndedOn("");
                                                        clearFeedback();
                                                    }}
                                                    style={({ pressed }) => [
                                                        styles.editButton,
                                                        pressed && !disabled
                                                            ? styles.buttonPressed
                                                            : null,
                                                        disabled
                                                            ? styles.buttonDisabled
                                                            : null,
                                                    ]}
                                                >
                                                    <Text style={styles.editButtonText}>
                                                        Befejezési dátum törlése
                                                    </Text>
                                                </Pressable>
                                            ) : null}
                                        </>
                                    ) : null}

                                    <FormField
                                        label="Megjegyzés"
                                        placeholder="Nem kötelező"
                                        multiline
                                        value={editNote}
                                        onChangeText={(value) => {
                                            setEditNote(value);
                                            clearFeedback();
                                        }}
                                        editable={!disabled}
                                    />

                                    <Text style={styles.help}>
                                        Az inaktiválás nem törli a korábbi műszakokat
                                        és bevételeket. A közös futárcég neve itt nem
                                        módosítható.
                                    </Text>

                                    <PrimaryButton
                                        title="Módosítások mentése"
                                        loading={isSubmitting}
                                        onPress={() => void handleUpdateCompany()}
                                    />

                                    {renderFeedback("edit")}

                                    <Pressable
                                        accessibilityRole="button"
                                        accessibilityState={{ disabled }}
                                        disabled={disabled}
                                        onPress={cancelEditing}
                                        style={({ pressed }) => [
                                            styles.editButton,
                                            pressed && !disabled
                                                ? styles.buttonPressed
                                                : null,
                                            disabled ? styles.buttonDisabled : null,
                                        ]}
                                    >
                                        <Text style={styles.editButtonText}>
                                            Mégse
                                        </Text>
                                    </Pressable>
                                </View>
                            ) : (
                                <>
                                    <Text style={styles.help}>
                                        Kezdés: {company.startedOn}
                                    </Text>

                                    {company.endedOn ? (
                                        <Text style={styles.help}>
                                            Befejezés: {company.endedOn}
                                        </Text>
                                    ) : null}

                                    {company.note ? (
                                        <Text style={styles.help}>
                                            {company.note}
                                        </Text>
                                    ) : null}

                                    {lastEditedId === company.id
                                        ? renderFeedback("edit")
                                        : null}
                                </>
                            )}
                        </View>
                    );
                })
            )}

            {!editingId ? (
                <>
                    <View style={styles.form}>
                        <Text style={styles.subheading}>
                            Meglévő cég hozzáadása
                        </Text>

                        {availableCompanies.length > 0 ? (
                            <>
                                <ChoiceField
                                    label="Futárcég"
                                    options={availableCompanies.map((company) => ({
                                        value: company.id,
                                        label: company.name,
                                    }))}
                                    value={companyId}
                                    onChange={(value) => {
                                        setCompanyId(value);
                                        clearFeedback();
                                    }}
                                    disabled={disabled}
                                />

                                <DateField
                                    label="Mióta dolgozol ennél a cégnél?"
                                    value={startedOn}
                                    onChange={(value) => {
                                        setStartedOn(value);
                                        clearFeedback();
                                    }}
                                    disabled={disabled}
                                />

                                <FormField
                                    label="Megjegyzés"
                                    placeholder="Nem kötelező"
                                    multiline
                                    value={note}
                                    onChangeText={(value) => {
                                        setNote(value);
                                        clearFeedback();
                                    }}
                                    editable={!disabled}
                                />

                                <PrimaryButton
                                    title="Hozzáadás a saját cégeimhez"
                                    loading={isSubmitting}
                                    onPress={() => void handleAddCompany()}
                                />
                            </>
                        ) : (
                            <Text style={styles.help}>
                                Nincs további kiválasztható cég. Ha hiányzik a
                                céged, lent felveheted a közös listába. A már
                                hozzárendelt inaktív céget a Szerkesztés gombbal
                                aktiválhatod újra.
                            </Text>
                        )}

                        {renderFeedback("add")}
                    </View>

                    <View style={styles.form}>
                        <Text style={styles.subheading}>
                            Nem találod a cégedet?
                        </Text>

                        <Text style={styles.help}>
                            Az új név a közös céglistába kerül. Előbb
                            ellenőrizd, hogy a cég nem szerepel-e már más
                            írásmóddal.
                        </Text>

                        <FormField
                            label="Új futárcég neve"
                            placeholder="A futárcég neve"
                            value={newCompanyName}
                            onChangeText={(value) => {
                                setNewCompanyName(value);
                                clearFeedback();
                            }}
                            editable={!disabled}
                        />

                        <PrimaryButton
                            title="Új futárcég létrehozása"
                            loading={isSubmitting}
                            onPress={() => void handleCreateCompany()}
                        />

                        {renderFeedback("create")}
                    </View>
                </>
            ) : null}
        </FadeInView>
    );
}

function createStyles(colors: ThemeColors) {
    return StyleSheet.create({
        section: {
            gap: 16,
            marginTop: 32,
        },
        heading: {
            color: colors.textPrimary,
            fontSize: 22,
            fontWeight: "700",
        },
        subheading: {
            color: colors.textPrimary,
            fontSize: 17,
            fontWeight: "600",
        },
        card: {
            backgroundColor: colors.surface,
            borderColor: colors.border,
            borderRadius: 14,
            borderWidth: 1,
            gap: 6,
            padding: 16,
        },
        editingCard: {
            borderColor: colors.primary,
        },
        cardHeader: {
            alignItems: "center",
            flexDirection: "row",
            flexWrap: "wrap",
            gap: 10,
            justifyContent: "space-between",
        },
        companyName: {
            color: colors.textPrimary,
            flexShrink: 1,
            fontSize: 17,
            fontWeight: "700",
        },
        statusBadge: {
            borderRadius: 20,
            paddingHorizontal: 10,
            paddingVertical: 4,
        },
        activeBadge: {
            backgroundColor: colors.surfaceMuted,
        },
        inactiveBadge: {
            backgroundColor: colors.surfaceMuted,
        },
        statusText: {
            fontSize: 12,
            fontWeight: "600",
        },
        activeText: {
            color: colors.success,
        },
        inactiveText: {
            color: colors.textSecondary,
        },
        help: {
            color: colors.textSecondary,
            fontSize: 13,
            lineHeight: 19,
        },
        form: {
            borderTopColor: colors.border,
            borderTopWidth: 1,
            gap: 14,
            paddingTop: 20,
        },
        inlineEditForm: {
            borderTopColor: colors.border,
            borderTopWidth: 1,
            gap: 14,
            marginTop: 10,
            paddingTop: 16,
        },
        feedback: {
            gap: 8,
        },
        editButton: {
            alignItems: "center",
            alignSelf: "flex-start",
            justifyContent: "center",
            minHeight: 44,
            paddingHorizontal: 12,
        },
        editButtonText: {
            color: colors.primary,
            fontSize: 14,
            fontWeight: "600",
        },
        buttonPressed: {
            opacity: 0.75,
        },
        buttonDisabled: {
            opacity: 0.6,
        },
        switchRow: {
            alignItems: "center",
            flexDirection: "row",
            gap: 16,
            justifyContent: "space-between",
            minHeight: 48,
        },
        switchLabel: {
            color: colors.textPrimary,
            flex: 1,
            fontSize: 15,
            fontWeight: "600",
        },
    });
}