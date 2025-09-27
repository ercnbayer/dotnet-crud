namespace dotnetcrud.Dto;

public class LoginResponse
{
    public required AuthenticatedUser User{get;set;} 
    public  required string AccessToken {get;set;}
    //public Guid RefreshToken;
}