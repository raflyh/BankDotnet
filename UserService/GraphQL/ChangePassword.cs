namespace UserService.GraphQL
{
    public record ChangePassword
    (
       // int Id,
        string Username,
        string OldPassword,
        string NewPassword
    );
}
