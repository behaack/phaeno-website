namespace phaeno.api.Common.Exceptions.Conflict;

public sealed class EmailAlreadyInUseException : ConflictException
{
    private const string DefaultMessage =
        "This Email is already in use.";
    
    public EmailAlreadyInUseException()
        : base(DefaultMessage) { }
    
    public override string ErrorCode =>
        "email_already_in_use";          
}
