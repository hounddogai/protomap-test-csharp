namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pulse survey adjustment is in its life cycle.</summary>
public enum ScheduledPulseSurveyAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled pulse survey adjustment as the pulse survey team keeps it.</summary>
public sealed class ScheduledPulseSurveyAdjustment(string code, string title, string reviewer, string timezone, int durationMinutes, double hours, int version, int attempts, int createdAt, double rate, string badge, ScheduledPulseSurveyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Timezone { get; set; } = timezone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Hours { get; set; } = hours;
    public int Version { get; set; } = version;
    public int Attempts { get; set; } = attempts;
    public int CreatedAt { get; set; } = createdAt;
    public double Rate { get; set; } = rate;
    public string Badge { get; set; } = badge;
    public ScheduledPulseSurveyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pulse survey adjustment in status planned.</summary>
    public static ScheduledPulseSurveyAdjustment NewScheduledPulseSurveyAdjustment(string code, string title)
    {
        return new ScheduledPulseSurveyAdjustment(
            code,
            title,
            "",
            "",
            0,
            0.0,
            0,
            0,
            0,
            0.0,
            "",
            ScheduledPulseSurveyAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPulseSurveyAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPulseSurveyAdjustmentStatus next)
    {
        switch (Status)
        {
            case ScheduledPulseSurveyAdjustmentStatus.Planned:
                switch (next)
                {
                    case ScheduledPulseSurveyAdjustmentStatus.Completed:
                    case ScheduledPulseSurveyAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledPulseSurveyAdjustmentStatus.Scheduled:
                switch (next)
                {
                    case ScheduledPulseSurveyAdjustmentStatus.InProgress:
                    case ScheduledPulseSurveyAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledPulseSurveyAdjustmentStatus.InProgress:
                switch (next)
                {
                    case ScheduledPulseSurveyAdjustmentStatus.Completed:
                    case ScheduledPulseSurveyAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledPulseSurveyAdjustmentStatus.Completed:
                return next == ScheduledPulseSurveyAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledPulseSurveyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pulse survey adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rate + (double)CreatedAt, 2);
    }

    public ScheduledPulseSurveyAdjustment Copy()
    {
        return new ScheduledPulseSurveyAdjustment(
            Code,
            Title,
            Reviewer,
            Timezone,
            DurationMinutes,
            Hours,
            Version,
            Attempts,
            CreatedAt,
            Rate,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
