namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling bonus pool slot is in its life cycle.</summary>
public enum RollingBonusPoolSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A rolling bonus pool slot as the bonus pool team keeps it.</summary>
public sealed class RollingBonusPoolSlot(string code, string title, double rate, int retries, string name, int leaveDays, string source, string salaryBand, double rating, int priority, RollingBonusPoolSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public int Retries { get; set; } = retries;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string Source { get; set; } = source;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rating { get; set; } = rating;
    public int Priority { get; set; } = priority;
    public RollingBonusPoolSlotStatus Status { get; set; } = status;

    /// <summary>Starts a rolling bonus pool slot in status requested.</summary>
    public static RollingBonusPoolSlot NewRollingBonusPoolSlot(string code, string title)
    {
        return new RollingBonusPoolSlot(
            code,
            title,
            0.0,
            0,
            "",
            0,
            "",
            "",
            0.0,
            0,
            RollingBonusPoolSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBonusPoolSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBonusPoolSlotStatus next)
    {
        switch (Status)
        {
            case RollingBonusPoolSlotStatus.Requested:
                return next == RollingBonusPoolSlotStatus.Expired;
            case RollingBonusPoolSlotStatus.Granted:
                switch (next)
                {
                    case RollingBonusPoolSlotStatus.Expired:
                        return true;
                }
                return false;
            case RollingBonusPoolSlotStatus.Revoked:
                return next == RollingBonusPoolSlotStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RollingBonusPoolSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling bonus pool slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)LeaveDays - (double)Retries, 2);
    }

    public RollingBonusPoolSlot Copy()
    {
        return new RollingBonusPoolSlot(
            Code,
            Title,
            Rate,
            Retries,
            Name,
            LeaveDays,
            Source,
            SalaryBand,
            Rating,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
