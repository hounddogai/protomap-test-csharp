namespace Acme.Workforce.Talent.Pool.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive pool assignment is in its life cycle.</summary>
public enum AdaptivePoolAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive pool assignment as the pool team keeps it.</summary>
public sealed class AdaptivePoolAssignment(string code, string label, int capacity, double minimum, string region, int graceDays, AdaptivePoolAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public double Minimum { get; set; } = minimum;
    public string Region { get; set; } = region;
    public int GraceDays { get; set; } = graceDays;
    public AdaptivePoolAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive pool assignment in status idle.</summary>
    public static AdaptivePoolAssignment NewAdaptivePoolAssignment(string code, string label)
    {
        return new AdaptivePoolAssignment(code, label, 0, 0.0, "", 0, AdaptivePoolAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePoolAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePoolAssignmentStatus next)
    {
        switch (Status)
        {
            case AdaptivePoolAssignmentStatus.Idle:
                return next == AdaptivePoolAssignmentStatus.Ready || next == AdaptivePoolAssignmentStatus.Stopped;
            case AdaptivePoolAssignmentStatus.Warming:
                return next == AdaptivePoolAssignmentStatus.Ready || next == AdaptivePoolAssignmentStatus.Stopped;
            case AdaptivePoolAssignmentStatus.Ready:
                return next == AdaptivePoolAssignmentStatus.Draining || next == AdaptivePoolAssignmentStatus.Stopped;
            case AdaptivePoolAssignmentStatus.Draining:
                return next == AdaptivePoolAssignmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptivePoolAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive pool assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Capacity * Minimum, 2);
    }

    public AdaptivePoolAssignment Copy()
    {
        return new AdaptivePoolAssignment(Code, Label, Capacity, Minimum, Region, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
