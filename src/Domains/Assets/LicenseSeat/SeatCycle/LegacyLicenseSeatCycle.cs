namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy license seat cycle is in its life cycle.</summary>
public enum LegacyLicenseSeatCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy license seat cycle. Its code is unique within a tenant.</summary>
public sealed class LegacyLicenseSeatCycle(string code, string name, int retries, double cost, bool pinned, int limit, double score, string reviewer, string description, int leaveDays, LegacyLicenseSeatCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public bool Pinned { get; set; } = pinned;
    public int Limit { get; set; } = limit;
    public double Score { get; set; } = score;
    public string Reviewer { get; set; } = reviewer;
    public string Description { get; set; } = description;
    public int LeaveDays { get; set; } = leaveDays;
    public LegacyLicenseSeatCycleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy license seat cycle in status planned.</summary>
    public static LegacyLicenseSeatCycle NewLegacyLicenseSeatCycle(string code, string name)
    {
        return new LegacyLicenseSeatCycle(
            code,
            name,
            0,
            0.0,
            false,
            0,
            0.0,
            "",
            "",
            0,
            LegacyLicenseSeatCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLicenseSeatCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLicenseSeatCycleStatus next)
    {
        switch (Status)
        {
            case LegacyLicenseSeatCycleStatus.Planned:
                return next == LegacyLicenseSeatCycleStatus.InProgress;
            case LegacyLicenseSeatCycleStatus.Scheduled:
                switch (next)
                {
                    case LegacyLicenseSeatCycleStatus.InProgress:
                        return true;
                }
                return false;
            case LegacyLicenseSeatCycleStatus.InProgress:
                switch (next)
                {
                    case LegacyLicenseSeatCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyLicenseSeatCycleStatus.Completed:
                switch (next)
                {
                    case LegacyLicenseSeatCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyLicenseSeatCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy license seat cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy license seat cycle"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Score + (double)LeaveDays, 2);
    }

    public LegacyLicenseSeatCycle Copy()
    {
        return new LegacyLicenseSeatCycle(
            Code,
            Name,
            Retries,
            Cost,
            Pinned,
            Limit,
            Score,
            Reviewer,
            Description,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
