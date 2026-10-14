namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled balance draft is in its life cycle.</summary>
public enum ScheduledBalanceDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One scheduled balance draft, with what the planners need to act on it.</summary>
public sealed class ScheduledBalanceDraft(string code, string name, int tenureMonths, int windowDays, double maximum, double score, string salaryBand, int durationMinutes, string costCenter, ScheduledBalanceDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public int WindowDays { get; set; } = windowDays;
    public double Maximum { get; set; } = maximum;
    public double Score { get; set; } = score;
    public string SalaryBand { get; set; } = salaryBand;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string CostCenter { get; set; } = costCenter;
    public ScheduledBalanceDraftStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled balance draft in status requested.</summary>
    public static ScheduledBalanceDraft NewScheduledBalanceDraft(string code, string name)
    {
        return new ScheduledBalanceDraft(
            code,
            name,
            0,
            0,
            0.0,
            0.0,
            "",
            0,
            "",
            ScheduledBalanceDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBalanceDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBalanceDraftStatus next)
    {
        switch (Status)
        {
            case ScheduledBalanceDraftStatus.Requested:
                switch (next)
                {
                    case ScheduledBalanceDraftStatus.Granted:
                        return true;
                }
                return false;
            case ScheduledBalanceDraftStatus.Granted:
                return next == ScheduledBalanceDraftStatus.Revoked || next == ScheduledBalanceDraftStatus.Expired;
            case ScheduledBalanceDraftStatus.Revoked:
                switch (next)
                {
                    case ScheduledBalanceDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledBalanceDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled balance draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled balance draft"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)DurationMinutes * Score, 2);
    }

    public ScheduledBalanceDraft Copy()
    {
        return new ScheduledBalanceDraft(
            Code,
            Name,
            TenureMonths,
            WindowDays,
            Maximum,
            Score,
            SalaryBand,
            DurationMinutes,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
