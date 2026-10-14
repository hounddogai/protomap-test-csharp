namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pool entry is in its life cycle.</summary>
public enum RollingPoolEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A rolling pool entry. Its code is unique within a tenant.</summary>
public sealed class RollingPoolEntry(string code, string title, double threshold, string segment, string category, RollingPoolEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Segment { get; set; } = segment;
    public string Category { get; set; } = category;
    public RollingPoolEntryStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pool entry in status proposed.</summary>
    public static RollingPoolEntry NewRollingPoolEntry(string code, string title)
    {
        return new RollingPoolEntry(code, title, 0.0, "", "", RollingPoolEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPoolEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPoolEntryStatus next)
    {
        switch (Status)
        {
            case RollingPoolEntryStatus.Proposed:
                switch (next)
                {
                    case RollingPoolEntryStatus.Rejected:
                    case RollingPoolEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingPoolEntryStatus.Approved:
                return next == RollingPoolEntryStatus.Withdrawn;
            case RollingPoolEntryStatus.Rejected:
                return next == RollingPoolEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingPoolEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pool entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling pool entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public RollingPoolEntry Copy()
    {
        return new RollingPoolEntry(Code, Title, Threshold, Segment, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
