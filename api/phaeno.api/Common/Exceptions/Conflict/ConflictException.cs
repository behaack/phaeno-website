namespace phaeno.api.Common.Exceptions.Conflict;

public abstract class ConflictException : DomainException
{
    protected ConflictException(string message)
        : base(message) {}

    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string ErrorType => "conflict";
}