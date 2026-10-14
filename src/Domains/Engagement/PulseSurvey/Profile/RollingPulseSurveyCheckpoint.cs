namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pulse survey checkpoint is in its life cycle.</summary>
public enum RollingPulseSurveyCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One rolling pulse survey checkpoint, with what the planners need to act on it.</summary>
public sealed class RollingPulseSurveyCheckpoint(string code, string title, int durationMinutes, string summary, int createdAt, double margin, string timezone, string name, string locale, int tenureMonths, RollingPulseSurveyCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Summary { get; set; } = summary;
    public int CreatedAt { get; set; } = createdAt;
    public double Margin { get; set; } = margin;
    public string Timezone { get; set; } = timezone;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public int TenureMonths { get; set; } = tenureMonths;
    public RollingPulseSurveyCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pulse survey checkpoint in status draft.</summary>
    public static RollingPulseSurveyCheckpoint NewRollingPulseSurveyCheckpoint(string code, string title)
    {
        return new RollingPulseSurveyCheckpoint(
            code,
            title,
            0,
            "",
            0,
            0.0,
            "",
            "",
            "",
            0,
            RollingPulseSurveyCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPulseSurveyCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPulseSurveyCheckpointStatus next)
    {
        switch (Status)
        {
            case RollingPulseSurveyCheckpointStatus.Draft:
                switch (next)
                {
                    case RollingPulseSurveyCheckpointStatus.Suspended:
                    case RollingPulseSurveyCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case RollingPulseSurveyCheckpointStatus.Pending:
                return next == RollingPulseSurveyCheckpointStatus.Active || next == RollingPulseSurveyCheckpointStatus.Closed;
            case RollingPulseSurveyCheckpointStatus.Active:
                switch (next)
                {
                    case RollingPulseSurveyCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case RollingPulseSurveyCheckpointStatus.Suspended:
                return next == RollingPulseSurveyCheckpointStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingPulseSurveyCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pulse survey checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling pulse survey checkpoint"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Margin * (double)CreatedAt, 2);
    }

    public RollingPulseSurveyCheckpoint Copy()
    {
        return new RollingPulseSurveyCheckpoint(
            Code,
            Title,
            DurationMinutes,
            Summary,
            CreatedAt,
            Margin,
            Timezone,
            Name,
            Locale,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
