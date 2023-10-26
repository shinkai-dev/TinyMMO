public abstract class EndpointConsts
{
	public static readonly string FIREBASE_REGISTER = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" + Secrets.FIREBASE_API_KEY;
	public static readonly string FIREBASE_LOGIN = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" + Secrets.FIREBASE_API_KEY;
}
