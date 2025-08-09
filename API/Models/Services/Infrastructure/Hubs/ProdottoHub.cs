using Microsoft.AspNetCore.SignalR;

namespace API.Models.Services.Infrastructure.Hubs
{
    public class ProdottoHub : Hub
    {
        public async Task NotificaQuantitaDisponibileAggiornata(int prodottoId, int qtDisponibile)
        {
            await Clients.All.SendAsync("QuantitaDisponibileAggiornata", prodottoId, qtDisponibile);
        }

        public async Task NotificaQuantitaMagazzinoAggiornata(int prodottoId, int qtMagazzino)
        {
            await Clients.All.SendAsync("QuantitaMagazzinoAggiornata", prodottoId, qtMagazzino);
        }
    }
}