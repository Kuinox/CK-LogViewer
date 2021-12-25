import { Api } from "../backend/api";

export function isPublicInstance(): boolean {
    return window.location.hostname == "log.kuinox.io";
}

export async function openOnPublicInstance(api: Api): Promise<void> {
    const newUri = await api.uploadLogToPublicInstance();
    window.location.href = newUri;
}
