const input = document.getElementById('addressInput'); // casella testo autocompletamento indirizzo
const suggestionsContainer = document.getElementById('suggestions'); // casella suggerimenti

const hiddenStreet = document.getElementById('hiddenStreet'); // campo nascosto indirizzo.Via 
const hiddenCity = document.getElementById('hiddenCity'); // campo nascosto indirizzo.Citta
const hiddenCAP = document.getElementById('hiddenCAP'); // campo nascosto indirizzo.CAP
const hiddenHouseNumber = document.getElementById('hiddenHouseNumber'); // campo nascosto inditizzo.HouseNumber

let controller = null;

input.addEventListener('input', async (e) => {
    const query = e.target.value.trim();
    if(!query)
    {
        suggestionsContainer.innerHTML = '';
        suggestionsContainer.classList.add('hidden');
        return;
    }

    if(controller) controller.abort();
    controller = new AbortController();

    try 
    {
        const response = await fetch(`http://localhost:5150/api/address/google-places-address?query=${encodeURIComponent(query)}&lang=it`, {
            signal: controller.signal,
        });

        if(!response.ok) throw new Error("Errore nella chiamata api");

        const results = await response.json();
        console.log("Results: " +results);

        if(!Array.isArray(results) || results.length === 0)
        {
            suggestionsContainer.innerHTML = "";
            suggestionsContainer.classList.add('hidden');
            return;
        }

        suggestionsContainer.innerHTML = results.map(result => `
            <div class="bg-white p-2 hover:bg-gray-100 cursor-pointer border-b border-gray-200"
                data-full='${JSON.stringify(result).replace(/'/g, "&apos;")}'>
                ${result.fullAddress || result.FullAddress}
            </div>
        `).join("");

        suggestionsContainer.classList.remove("hidden");

        suggestionsContainer.querySelectorAll("div").forEach(item => {
            item.addEventListener("click", () => {
                const data = JSON.parse(item.getAttribute("data-full").replace(/&apos;/g, "'"));

                // Popola input visibile
                input.value = data.fullAddress || data.FullAddress;

                // Popola hidden fields
                hiddenStreet.value = data.street || "";
                hiddenCity.value = data.city || "";
                hiddenCAP.value = data.postalCode || "";
                hiddenHouseNumber.value = data.houseNumber || "";

                // Nascondi suggerimenti
                suggestionsContainer.classList.add("hidden");
            });
        });

    } catch(error)
    {
        if(error.name !== 'AbortError') console.error("Errore:" , error);

    }
});

// Chiudi casella suggerimenti al click su una qualsiasi parte dello schermo che non sia il contenitore di suggerimenti.
document.addEventListener("click", (e) => {
    if (!input.contains(e.target) && !suggestionsContainer.contains(e.target)) {
        suggestionsContainer.classList.add("hidden");
    }
});
 
