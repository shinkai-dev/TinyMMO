public partial class UserCollection : DbController<UserModel>
{
    public UserCollection(): base("users") { }
}