using Godot.Collections;

public abstract class ErrorMessageConsts
{
    public static readonly Dictionary<string, string> dictionary = new Dictionary<string, string>()
    {
        {"INVALID_EMAIL", "The email address is badly formatted."},
        {"INVALID_PASSWORD", "The password is invalid or the user does not have a password."},
        {"USER_DISABLED", "The user account has been disabled by an administrator."},
        {"EMAIL_EXISTS", "The email address is already in use by another account."},
        {"OPERATION_NOT_ALLOWED", "Password sign-in is disabled for this project."},
        {"INVALID_LOGIN_CREDENTIALS", "The email address or password is incorrect."},
        {"TOO_MANY_ATTEMPTS_TRY_LATER", "We have blocked all requests from this device due to unusual activity. Try again later."}
    };
}