namespace API.Models.DTO
{
    public class RegisterRequestDto
    {
        public string Nome { get; set; } 
        public string Cognome { get; set; } 
        public string Email { get; set; } 
        public string Password { get; set; }


        public RegisterRequestDto(string nome, string cognome, string email, string password)
        {
            Nome = nome;
            Cognome = cognome;
            Email = email;
            Password = password;
        }
        
    }
}
