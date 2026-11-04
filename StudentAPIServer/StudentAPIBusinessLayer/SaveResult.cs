public class SaveResult
{
    public enum enSaveResult
    {
        Success,
        ValidationFailed,
        NotFound,
        DatabaseError
    }


    public enSaveResult Status { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsSuccess => Status == enSaveResult.Success;

    public static SaveResult Success() => new() { Status = enSaveResult.Success };
    public static SaveResult ValidationFailed(string msg) => new() { Status = enSaveResult.ValidationFailed, ErrorMessage = msg };
    public static SaveResult NotFound(string msg) => new() { Status = enSaveResult.NotFound, ErrorMessage = msg };
    public static SaveResult DatabaseError(string msg) => new() { Status = enSaveResult.DatabaseError, ErrorMessage = msg };
}