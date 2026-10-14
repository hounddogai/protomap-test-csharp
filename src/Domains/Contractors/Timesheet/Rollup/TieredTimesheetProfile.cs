namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered timesheet profile is in its life cycle.</summary>
public enum TieredTimesheetProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One tiered timesheet profile, with what the planners need to act on it.</summary>
public sealed class TieredTimesheetProfile(string code, string title, int startsAt, int attempts, string label, double fte, double cost, int windowDays, int batchSize, string team, int tenureMonths, TieredTimesheetProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public int Attempts { get; set; } = attempts;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public double Cost { get; set; } = cost;
    public int WindowDays { get; set; } = windowDays;
    public int BatchSize { get; set; } = batchSize;
    public string Team { get; set; } = team;
    public int TenureMonths { get; set; } = tenureMonths;
    public TieredTimesheetProfileStatus Status { get; set; } = status;

    /// <summary>Starts a tiered timesheet profile in status requested.</summary>
    public static TieredTimesheetProfile NewTieredTimesheetProfile(string code, string title)
    {
        return new TieredTimesheetProfile(
            code,
            title,
            0,
            0,
            "",
            0.0,
            0.0,
            0,
            0,
            "",
            0,
            TieredTimesheetProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredTimesheetProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredTimesheetProfileStatus next)
    {
        switch (Status)
        {
            case TieredTimesheetProfileStatus.Requested:
                return next == TieredTimesheetProfileStatus.Granted;
            case TieredTimesheetProfileStatus.Granted:
                return next == TieredTimesheetProfileStatus.Revoked || next == TieredTimesheetProfileStatus.Expired;
            case TieredTimesheetProfileStatus.Revoked:
                switch (next)
                {
                    case TieredTimesheetProfileStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredTimesheetProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered timesheet profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tiered timesheet profile"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)BatchSize + (double)Attempts, 2);
    }

    public TieredTimesheetProfile Copy()
    {
        return new TieredTimesheetProfile(
            Code,
            Title,
            StartsAt,
            Attempts,
            Label,
            Fte,
            Cost,
            WindowDays,
            BatchSize,
            Team,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
