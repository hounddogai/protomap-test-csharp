namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pool assignment is in its life cycle.</summary>
public enum FallbackPoolAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a fallback pool assignment and its current status.</summary>
public sealed class FallbackPoolAssignment(string code, string name, string channel, string segment, double floorValue, double minimum, FallbackPoolAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public string Segment { get; set; } = segment;
    public double FloorValue { get; set; } = floorValue;
    public double Minimum { get; set; } = minimum;
    public FallbackPoolAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pool assignment in status draft.</summary>
    public static FallbackPoolAssignment NewFallbackPoolAssignment(string code, string name)
    {
        return new FallbackPoolAssignment(code, name, "", "", 0.0, 0.0, FallbackPoolAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPoolAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPoolAssignmentStatus next)
    {
        switch (Status)
        {
            case FallbackPoolAssignmentStatus.Draft:
                return next == FallbackPoolAssignmentStatus.Pending || next == FallbackPoolAssignmentStatus.Closed;
            case FallbackPoolAssignmentStatus.Pending:
                return next == FallbackPoolAssignmentStatus.Suspended;
            case FallbackPoolAssignmentStatus.Active:
                return next == FallbackPoolAssignmentStatus.Closed;
            case FallbackPoolAssignmentStatus.Suspended:
                return next == FallbackPoolAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FallbackPoolAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pool assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback pool assignment"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(FloorValue - Minimum, 2);
    }

    public FallbackPoolAssignment Copy()
    {
        return new FallbackPoolAssignment(Code, Name, Channel, Segment, FloorValue, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
