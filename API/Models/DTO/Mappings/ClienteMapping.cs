namespace API.Models.DTO.Mappings;

public static class ClienteMapping
{
    public static Cliente ToEntity(this ClienteDto clienteDto)
    {
        return new Cliente
        {
            Id = clienteDto.Id,
            Nome = clienteDto.Nome,
            Cognome = clienteDto.Cognome,
            Email = clienteDto.Email,
            NumeroTelefono = clienteDto.NumeroTelefono,
            Indirizzo = clienteDto.Indirizzo,
            DataIscrizione = clienteDto.DataIscrizione
        };
    }

    public static ClienteDto ToDto(this Cliente cliente)
    {
        return new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Cognome = cliente.Cognome,
            Email = cliente.Email,
            NumeroTelefono = cliente.NumeroTelefono,
            Indirizzo = cliente.Indirizzo,
            DataIscrizione = cliente.DataIscrizione
        };
    }
}
