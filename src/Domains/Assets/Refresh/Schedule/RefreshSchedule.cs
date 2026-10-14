namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh schedule is in its life cycle.</summary>
public enum RefreshScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A refresh schedule as the refresh team keeps it.</summary>
public sealed class RefreshSchedule(string code, string title, double ratio, string description, int windowDays, string country, string locale, string team, double maximum, double ceiling, bool pinned, RefreshScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Description { get; set; } = description;
    public int WindowDays { get; set; } = windowDays;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public string Team { get; set; } = team;
    public double Maximum { get; set; } = maximum;
    public double Ceiling { get; set; } = ceiling;
    public bool Pinned { get; set; } = pinned;
    public RefreshScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a refresh schedule in status planned.</summary>
    public static RefreshSchedule NewRefreshSchedule(string code, string title)
    {
        return new RefreshSchedule(
            code,
            title,
            0.0,
            "",
            0,
            "",
            "",
            "",
            0.0,
            0.0,
            false,
            RefreshScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshScheduleStatus next)
    {
        switch (Status)
        {
            case RefreshScheduleStatus.Planned:
                return next == RefreshScheduleStatus.InProgress;
            case RefreshScheduleStatus.Scheduled:
                switch (next)
                {
                    case RefreshScheduleStatus.InProgress:
                        return true;
                }
                return false;
            case RefreshScheduleStatus.InProgress:
                switch (next)
                {
                    case RefreshScheduleStatus.Completed:
                        return true;
                }
                return false;
            case RefreshScheduleStatus.Completed:
                return next == RefreshScheduleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RefreshScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"refresh schedule"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)WindowDays - Maximum, 2);
    }

    public RefreshSchedule Copy()
    {
        return new RefreshSchedule(
            Code,
            Title,
            Ratio,
            Description,
            WindowDays,
            Country,
            Locale,
            Team,
            Maximum,
            Ceiling,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
