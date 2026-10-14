namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle mapping is in its life cycle.</summary>
public enum MeritCycleMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One merit cycle mapping, with what the planners need to act on it.</summary>
public sealed class MeritCycleMapping(string code, string title, string grade, string region, double margin, int graceDays, int windowDays, int limit, string unit, bool pinned, int revision, MeritCycleMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string Region { get; set; } = region;
    public double Margin { get; set; } = margin;
    public int GraceDays { get; set; } = graceDays;
    public int WindowDays { get; set; } = windowDays;
    public int Limit { get; set; } = limit;
    public string Unit { get; set; } = unit;
    public bool Pinned { get; set; } = pinned;
    public int Revision { get; set; } = revision;
    public MeritCycleMappingStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle mapping in status draft.</summary>
    public static MeritCycleMapping NewMeritCycleMapping(string code, string title)
    {
        return new MeritCycleMapping(code, title, "", "", 0.0, 0, 0, 0, "", false, 0, MeritCycleMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleMappingStatus next)
    {
        switch (Status)
        {
            case MeritCycleMappingStatus.Draft:
                return next == MeritCycleMappingStatus.Suspended;
            case MeritCycleMappingStatus.Pending:
                return next == MeritCycleMappingStatus.Active;
            case MeritCycleMappingStatus.Active:
                return next == MeritCycleMappingStatus.Suspended;
            case MeritCycleMappingStatus.Suspended:
                return next == MeritCycleMappingStatus.Closed;
        }
        return false;
    }

    public void MoveTo(MeritCycleMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"merit cycle mapping"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)GraceDays + (double)WindowDays, 2);
    }

    public MeritCycleMapping Copy()
    {
        return new MeritCycleMapping(
            Code,
            Title,
            Grade,
            Region,
            Margin,
            GraceDays,
            WindowDays,
            Limit,
            Unit,
            Pinned,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
