namespace ScheduleManagement.Api.Common;

public sealed class ScheduleValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ScheduleValidationException(
        IReadOnlyDictionary<string, string[]> errors)
        : base("One or more schedule validation errors occurred.")
    {
        Errors = errors;
    }
}