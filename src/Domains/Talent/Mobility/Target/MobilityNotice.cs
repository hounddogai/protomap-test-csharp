namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility notice is in its life cycle.</summary>
public enum MobilityNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A mobility notice as the mobility team keeps it.</summary>
public sealed class MobilityNotice(string code, string name, int limit, int priority, string unit, int tenureMonths, double cost, int windowDays, double amount, double rate, MobilityNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public int Priority { get; set; } = priority;
    public string Unit { get; set; } = unit;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public int WindowDays { get; set; } = windowDays;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public MobilityNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a mobility notice in status proposed.</summary>
    public static MobilityNotice NewMobilityNotice(string code, string name)
    {
        return new MobilityNotice(code, name, 0, 0, "", 0, 0.0, 0, 0.0, 0.0, MobilityNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityNoticeStatus next)
    {
        switch (Status)
        {
            case MobilityNoticeStatus.Proposed:
                return next == MobilityNoticeStatus.Withdrawn;
            case MobilityNoticeStatus.Approved:
                return next == MobilityNoticeStatus.Withdrawn;
            case MobilityNoticeStatus.Rejected:
                switch (next)
                {
                    case MobilityNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Rate - (double)TenureMonths, 2);
    }

    public MobilityNotice Copy()
    {
        return new MobilityNotice(
            Code,
            Name,
            Limit,
            Priority,
            Unit,
            TenureMonths,
            Cost,
            WindowDays,
            Amount,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
