namespace ScheduleManagement.Api.Common;

public sealed class DuplicateScheduleException : Exception
{
    public DuplicateScheduleException()
        : base(
            "A schedule with the same flight number and effective from date already exists.")
    {
    }
}