namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool assignment is in its life cycle.</summary>
public enum PoolAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pool assignment, with what the planners need to act on it.</summary>
public sealed class PoolAssignment(string code, string label, int version, string currency, string description, string externalRef, string source, int limit, double rate, PoolAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Currency { get; set; } = currency;
    public string Description { get; set; } = description;
    public string ExternalRef { get; set; } = externalRef;
    public string Source { get; set; } = source;
    public int Limit { get; set; } = limit;
    public double Rate { get; set; } = rate;
    public PoolAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a pool assignment in status proposed.</summary>
    public static PoolAssignment NewPoolAssignment(string code, string label)
    {
        return new PoolAssignment(code, label, 0, "", "", "", "", 0, 0.0, PoolAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolAssignmentStatus next)
    {
        switch (Status)
        {
            case PoolAssignmentStatus.Proposed:
                switch (next)
                {
                    case PoolAssignmentStatus.Rejected:
                    case PoolAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolAssignmentStatus.Approved:
                return next == PoolAssignmentStatus.Rejected;
            case PoolAssignmentStatus.Rejected:
                return next == PoolAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PoolAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Limit * (double)Version, 2);
    }

    public PoolAssignment Copy()
    {
        return new PoolAssignment(
            Code,
            Label,
            Version,
            Currency,
            Description,
            ExternalRef,
            Source,
            Limit,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
