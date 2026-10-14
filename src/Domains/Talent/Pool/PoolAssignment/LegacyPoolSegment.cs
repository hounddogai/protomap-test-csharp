namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pool segment is in its life cycle.</summary>
public enum LegacyPoolSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy pool segment as the pool team keeps it.</summary>
public sealed class LegacyPoolSegment(string code, string name, double threshold, string externalRef, string label, string category, LegacyPoolSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string ExternalRef { get; set; } = externalRef;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public LegacyPoolSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pool segment in status planned.</summary>
    public static LegacyPoolSegment NewLegacyPoolSegment(string code, string name)
    {
        return new LegacyPoolSegment(code, name, 0.0, "", "", "", LegacyPoolSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPoolSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPoolSegmentStatus next)
    {
        switch (Status)
        {
            case LegacyPoolSegmentStatus.Planned:
                return next == LegacyPoolSegmentStatus.Completed;
            case LegacyPoolSegmentStatus.Scheduled:
                switch (next)
                {
                    case LegacyPoolSegmentStatus.InProgress:
                    case LegacyPoolSegmentStatus.Completed:
                        return true;
                }
                return false;
            case LegacyPoolSegmentStatus.InProgress:
                return next == LegacyPoolSegmentStatus.Completed || next == LegacyPoolSegmentStatus.Cancelled;
            case LegacyPoolSegmentStatus.Completed:
                switch (next)
                {
                    case LegacyPoolSegmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPoolSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pool segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public LegacyPoolSegment Copy()
    {
        return new LegacyPoolSegment(Code, Name, Threshold, ExternalRef, Label, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
