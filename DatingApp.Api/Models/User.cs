namespace DatingApp.Api.Models
{
    public class User
    {
        public int Id {get; set;}

        public string Username {get; set;}

        public byte[] Passwordhash {get; set;}
        
        public byte[] PasswordSalt {get; set;}
    }
}