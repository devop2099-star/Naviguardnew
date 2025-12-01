// Naviguard.Application/Common/Result.cs
namespace Naviguard.Application.Common
{
    /// <summary>
    /// Representa el resultado de una operación sin valor de retorno
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Un resultado exitoso no puede tener un error");

            if (!isSuccess && string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Un resultado fallido debe tener un error");

            IsSuccess = isSuccess;
            Error = error ?? string.Empty;
        }

        // Métodos para Result sin tipo genérico
        public static Result Success() => new Result(true, string.Empty);

        public static Result Failure(string error) => new Result(false, error);
    }

    /// <summary>
    /// Representa el resultado de una operación con valor de retorno
    /// </summary>
    public class Result<T> : Result
    {
        public T? Value { get; }

        protected Result(T? value, bool isSuccess, string error)
            : base(isSuccess, error)
        {
            Value = value;
        }

        // Métodos estáticos para Result<T>
        public static Result<T> Success(T value) =>
            new Result<T>(value, true, string.Empty);

        public static Result<T> Failure(string error) =>
            new Result<T>(default, false, error);
    }
}