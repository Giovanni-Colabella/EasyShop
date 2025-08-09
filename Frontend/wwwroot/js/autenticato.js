

// questo script controlla se l'utente è autenticato
// e ritorna anche i ruoli dell'utente
async function checkAuth() {
    try {
        const response = await fetch("http://localhost:5150/api/auth/isAuthenticated", {
            method: "GET",
            credentials: "include"
        });

        if (response.status === 401) {
            return { isAuthenticated: false, roles: [] };
        }

        // Controlla se la risposta è JSON valido
        const contentType = response.headers.get("Content-Type");
        if (!response.ok || !contentType?.includes("application/json")) {
            console.error("Risposta non valida:", response.status);
            return { isAuthenticated: false, roles: [] };
        }

        const data = await response.json();
        console.log("Risposta completa:", data);

        // Usa camelCase per le proprietà
        return {
            isAuthenticated: data.isAuthenticated ?? false,
            roles: data.roles ?? []
        };

    } catch (error) {
        console.error("Errore nella richiesta:", error);
        return { isAuthenticated: false, roles: [] };
    }
}
