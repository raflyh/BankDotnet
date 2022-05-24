namespace UserService.GraphQL
{
    public record UpdateUser
    (
        string FullName,
        string Username, 
        string PhoneNumber, 
        string Address 
    );
}
