namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition schedule is in its life cycle.</summary>
public enum RecognitionScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a recognition schedule and its current status.</summary>
public sealed class RecognitionSchedule(string code, string title, string description, string timezone, int graceDays, RecognitionScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Timezone { get; set; } = timezone;
    public int GraceDays { get; set; } = graceDays;
    public RecognitionScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a recognition schedule in status open.</summary>
    public static RecognitionSchedule NewRecognitionSchedule(string code, string title)
    {
        return new RecognitionSchedule(code, title, "", "", 0, RecognitionScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionScheduleStatus next)
    {
        switch (Status)
        {
            case RecognitionScheduleStatus.Open:
                return next == RecognitionScheduleStatus.InReview || next == RecognitionScheduleStatus.Reopened;
            case RecognitionScheduleStatus.InReview:
                return next == RecognitionScheduleStatus.Reopened || next == RecognitionScheduleStatus.Archived;
            case RecognitionScheduleStatus.Resolved:
                switch (next)
                {
                    case RecognitionScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case RecognitionScheduleStatus.Reopened:
                switch (next)
                {
                    case RecognitionScheduleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public RecognitionSchedule Copy()
    {
        return new RecognitionSchedule(Code, Title, Description, Timezone, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
