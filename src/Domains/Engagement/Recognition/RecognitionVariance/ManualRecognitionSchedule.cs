namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual recognition schedule is in its life cycle.</summary>
public enum ManualRecognitionScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One manual recognition schedule, with what the planners need to act on it.</summary>
public sealed class ManualRecognitionSchedule(string code, string title, string location, double rating, bool approved, ManualRecognitionScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public double Rating { get; set; } = rating;
    public bool Approved { get; set; } = approved;
    public ManualRecognitionScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a manual recognition schedule in status created.</summary>
    public static ManualRecognitionSchedule NewManualRecognitionSchedule(string code, string title)
    {
        return new ManualRecognitionSchedule(code, title, "", 0.0, false, ManualRecognitionScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRecognitionScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRecognitionScheduleStatus next)
    {
        switch (Status)
        {
            case ManualRecognitionScheduleStatus.Created:
                return next == ManualRecognitionScheduleStatus.Published || next == ManualRecognitionScheduleStatus.Retired;
            case ManualRecognitionScheduleStatus.Validated:
                return next == ManualRecognitionScheduleStatus.Retired;
            case ManualRecognitionScheduleStatus.Published:
                return next == ManualRecognitionScheduleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualRecognitionScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual recognition schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual recognition schedule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public ManualRecognitionSchedule Copy()
    {
        return new ManualRecognitionSchedule(Code, Title, Location, Rating, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
