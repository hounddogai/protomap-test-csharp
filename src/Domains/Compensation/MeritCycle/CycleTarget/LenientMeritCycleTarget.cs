namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient merit cycle target is in its life cycle.</summary>
public enum LenientMeritCycleTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One lenient merit cycle target, with what the planners need to act on it.</summary>
public sealed class LenientMeritCycleTarget(string code, string name, double weight, Dictionary<string, string> attributes, int limit, string costCenter, double floorValue, LenientMeritCycleTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Limit { get; set; } = limit;
    public string CostCenter { get; set; } = costCenter;
    public double FloorValue { get; set; } = floorValue;
    public LenientMeritCycleTargetStatus Status { get; set; } = status;

    /// <summary>Starts a lenient merit cycle target in status planned.</summary>
    public static LenientMeritCycleTarget NewLenientMeritCycleTarget(string code, string name)
    {
        return new LenientMeritCycleTarget(
            code,
            name,
            0.0,
            new Dictionary<string, string>(),
            0,
            "",
            0.0,
            LenientMeritCycleTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientMeritCycleTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientMeritCycleTargetStatus next)
    {
        switch (Status)
        {
            case LenientMeritCycleTargetStatus.Planned:
                return next == LenientMeritCycleTargetStatus.Scheduled;
            case LenientMeritCycleTargetStatus.Scheduled:
                return next == LenientMeritCycleTargetStatus.Completed || next == LenientMeritCycleTargetStatus.Cancelled;
            case LenientMeritCycleTargetStatus.InProgress:
                return next == LenientMeritCycleTargetStatus.Cancelled;
            case LenientMeritCycleTargetStatus.Completed:
                return next == LenientMeritCycleTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientMeritCycleTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient merit cycle target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient merit cycle target"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue - Weight, 2);
    }

    public LenientMeritCycleTarget Copy()
    {
        return new LenientMeritCycleTarget(Code, Name, Weight, Attributes, Limit, CostCenter, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
