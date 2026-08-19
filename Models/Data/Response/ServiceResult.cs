namespace _Tripfinity.Models.Data.Response;

public record ServiceResult(bool Success, string Message)
{
    public static ServiceResult Ok(string message = "") => new(true, message);
    public static ServiceResult Fail(string message) => new(false, message);
}