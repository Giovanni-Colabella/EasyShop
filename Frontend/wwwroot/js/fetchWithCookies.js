
// Questa funzione ha come obiettivo quello di essere un comune fetch js 
// ma che includa i cookie di default


// Inutile per il momento, fetch di default include i cookie
// Non ha alcun utilizzo nell'app , ma lo lascio per completezza
export async function fetchWithCookies(url, options = {}) {
    
    const baseOptions = {
        credentials: 'include'
    };

    const completeOptions = { ...baseOptions, ...options};

    return await fetch(url, completeOptions);
}