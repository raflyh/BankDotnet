namespace UserService.GraphQL
{
    public record RegisterUser
    (
        string FullName,
        string UserName,
        string Password,      
        string PhoneNumber,
        string Address 
    );
}
