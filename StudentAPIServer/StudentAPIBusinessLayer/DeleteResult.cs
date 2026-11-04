namespace StudentAPIBusinessLayer
{
    public class DeleteResult
    {
        public enum enDeleteResult
        {
            Success,
            NotFound,
            DatabaseError,
            InvalidInput
        }

        public enDeleteResult Status { get; init; }
        public string? ErrorMessage { get; init; }
        public bool IsSuccess => Status == enDeleteResult.Success;

        public static DeleteResult Success() => new() { Status = enDeleteResult.Success };
        public static DeleteResult NotFound(string msg) => new() { Status = enDeleteResult.NotFound, ErrorMessage = msg };
        public static DeleteResult DatabaseError(string msg) => new() { Status = enDeleteResult.DatabaseError, ErrorMessage = msg };
        public static DeleteResult InvalidInput(string msg) => new() { Status = enDeleteResult.InvalidInput, ErrorMessage = msg };
    }
}


