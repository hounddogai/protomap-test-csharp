namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position profile is in its life cycle.</summary>
public enum PositionProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A position profile. Its code is unique within a tenant.</summary>
public sealed class PositionProfile(string code, string name, string summary, int startsAt, string segment, int leaveDays, double maximum, PositionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public int StartsAt { get; set; } = startsAt;
    public string Segment { get; set; } = segment;
    public int LeaveDays { get; set; } = leaveDays;
    public double Maximum { get; set; } = maximum;
    public PositionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a position profile in status planned.</summary>
    public static PositionProfile NewPositionProfile(string code, string name)
    {
        return new PositionProfile(code, name, "", 0, "", 0, 0.0, PositionProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionProfileStatus next)
    {
        switch (Status)
        {
            case PositionProfileStatus.Planned:
                return next == PositionProfileStatus.Scheduled || next == PositionProfileStatus.Completed;
            case PositionProfileStatus.Scheduled:
                return next == PositionProfileStatus.Cancelled;
            case PositionProfileStatus.InProgress:
                return next == PositionProfileStatus.Completed || next == PositionProfileStatus.Cancelled;
            case PositionProfileStatus.Completed:
                return next == PositionProfileStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PositionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"position profile"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)StartsAt * Maximum, 2);
    }

    public PositionProfile Copy()
    {
        return new PositionProfile(Code, Name, Summary, StartsAt, Segment, LeaveDays, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
