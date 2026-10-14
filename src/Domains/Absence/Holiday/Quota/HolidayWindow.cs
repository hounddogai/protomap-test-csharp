namespace Acme.Workforce.Absence.Holiday.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday window is in its life cycle.</summary>
public enum HolidayWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A holiday window. Its code is unique within a tenant.</summary>
public sealed class HolidayWindow(string code, string name, double minimum, double ratio, double rate, int durationMinutes, bool active, int expiresAt, double threshold, HolidayWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public double Ratio { get; set; } = ratio;
    public double Rate { get; set; } = rate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Active { get; set; } = active;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Threshold { get; set; } = threshold;
    public HolidayWindowStatus Status { get; set; } = status;

    /// <summary>Starts a holiday window in status open.</summary>
    public static HolidayWindow NewHolidayWindow(string code, string name)
    {
        return new HolidayWindow(code, name, 0.0, 0.0, 0.0, 0, false, 0, 0.0, HolidayWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayWindowStatus next)
    {
        switch (Status)
        {
            case HolidayWindowStatus.Open:
                switch (next)
                {
                    case HolidayWindowStatus.InReview:
                    case HolidayWindowStatus.Resolved:
                        return true;
                }
                return false;
            case HolidayWindowStatus.InReview:
                return next == HolidayWindowStatus.Archived;
            case HolidayWindowStatus.Resolved:
                return next == HolidayWindowStatus.Reopened || next == HolidayWindowStatus.Archived;
            case HolidayWindowStatus.Reopened:
                switch (next)
                {
                    case HolidayWindowStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"holiday window"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Threshold - (double)DurationMinutes, 2);
    }

    public HolidayWindow Copy()
    {
        return new HolidayWindow(
            Code,
            Name,
            Minimum,
            Ratio,
            Rate,
            DurationMinutes,
            Active,
            ExpiresAt,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
