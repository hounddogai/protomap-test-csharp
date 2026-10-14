namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pool baseline is in its life cycle.</summary>
public enum GlobalPoolBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One global pool baseline, with what the planners need to act on it.</summary>
public sealed class GlobalPoolBaseline(string code, string name, int attempts, string timezone, bool pinned, double floorValue, double ratio, GlobalPoolBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Timezone { get; set; } = timezone;
    public bool Pinned { get; set; } = pinned;
    public double FloorValue { get; set; } = floorValue;
    public double Ratio { get; set; } = ratio;
    public GlobalPoolBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a global pool baseline in status planned.</summary>
    public static GlobalPoolBaseline NewGlobalPoolBaseline(string code, string name)
    {
        return new GlobalPoolBaseline(code, name, 0, "", false, 0.0, 0.0, GlobalPoolBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPoolBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPoolBaselineStatus next)
    {
        switch (Status)
        {
            case GlobalPoolBaselineStatus.Planned:
                return next == GlobalPoolBaselineStatus.Scheduled || next == GlobalPoolBaselineStatus.Cancelled;
            case GlobalPoolBaselineStatus.Scheduled:
                switch (next)
                {
                    case GlobalPoolBaselineStatus.Completed:
                        return true;
                }
                return false;
            case GlobalPoolBaselineStatus.InProgress:
                return next == GlobalPoolBaselineStatus.Completed || next == GlobalPoolBaselineStatus.Cancelled;
            case GlobalPoolBaselineStatus.Completed:
                switch (next)
                {
                    case GlobalPoolBaselineStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPoolBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pool baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round(FloorValue - (double)Attempts, 2);
    }

    public GlobalPoolBaseline Copy()
    {
        return new GlobalPoolBaseline(Code, Name, Attempts, Timezone, Pinned, FloorValue, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
