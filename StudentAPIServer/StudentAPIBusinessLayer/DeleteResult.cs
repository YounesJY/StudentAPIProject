public class DeleteResult
{
    public enum enDeleteResult
    {
        Success,
        NotFound,
        DatabaseError
    }

    public enDeleteResult Status { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsSuccess => Status == enDeleteResult.Success;

    public static DeleteResult Success() => new() { Status = enDeleteResult.Success };
    public static DeleteResult NotFound(string msg) => new() { Status = enDeleteResult.NotFound, ErrorMessage = msg };
    public static DeleteResult DatabaseError(string msg) => new() { Status = enDeleteResult.DatabaseError, ErrorMessage = msg };
}
