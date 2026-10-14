namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient merit cycle window is in its life cycle.</summary>
public enum LenientMeritCycleWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient merit cycle window as the merit cycle team keeps it.</summary>
public sealed class LenientMeritCycleWindow(string code, string name, int windowDays, string summary, double maximum, int leaveDays, string ownerId, double discount, LenientMeritCycleWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Summary { get; set; } = summary;
    public double Maximum { get; set; } = maximum;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public LenientMeritCycleWindowStatus Status { get; set; } = status;

    /// <summary>Starts a lenient merit cycle window in status draft.</summary>
    public static LenientMeritCycleWindow NewLenientMeritCycleWindow(string code, string name)
    {
        return new LenientMeritCycleWindow(code, name, 0, "", 0.0, 0, "", 0.0, LenientMeritCycleWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientMeritCycleWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientMeritCycleWindowStatus next)
    {
        switch (Status)
        {
            case LenientMeritCycleWindowStatus.Draft:
                switch (next)
                {
                    case LenientMeritCycleWindowStatus.Suspended:
                        return true;
                }
                return false;
            case LenientMeritCycleWindowStatus.Pending:
                return next == LenientMeritCycleWindowStatus.Active || next == LenientMeritCycleWindowStatus.Closed;
            case LenientMeritCycleWindowStatus.Active:
                switch (next)
                {
                    case LenientMeritCycleWindowStatus.Suspended:
                    case LenientMeritCycleWindowStatus.Closed:
                        return true;
                }
                return false;
            case LenientMeritCycleWindowStatus.Suspended:
                return next == LenientMeritCycleWindowStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientMeritCycleWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient merit cycle window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)LeaveDays - (double)WindowDays, 2);
    }

    public LenientMeritCycleWindow Copy()
    {
        return new LenientMeritCycleWindow(
            Code,
            Name,
            WindowDays,
            Summary,
            Maximum,
            LeaveDays,
            OwnerId,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
