namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack snapshot is in its life cycle.</summary>
public enum PolicyAckSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A policy ack snapshot as the policy ack team keeps it.</summary>
public sealed class PolicyAckSnapshot(string code, string label, Dictionary<string, string> attributes, double rate, int attempts, string category, double budget, PolicyAckSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public string Category { get; set; } = category;
    public double Budget { get; set; } = budget;
    public PolicyAckSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack snapshot in status open.</summary>
    public static PolicyAckSnapshot NewPolicyAckSnapshot(string code, string label)
    {
        return new PolicyAckSnapshot(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            0.0,
            PolicyAckSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckSnapshotStatus next)
    {
        switch (Status)
        {
            case PolicyAckSnapshotStatus.Open:
                switch (next)
                {
                    case PolicyAckSnapshotStatus.Resolved:
                        return true;
                }
                return false;
            case PolicyAckSnapshotStatus.InReview:
                switch (next)
                {
                    case PolicyAckSnapshotStatus.Resolved:
                    case PolicyAckSnapshotStatus.Reopened:
                        return true;
                }
                return false;
            case PolicyAckSnapshotStatus.Resolved:
                return next == PolicyAckSnapshotStatus.Reopened || next == PolicyAckSnapshotStatus.Archived;
            case PolicyAckSnapshotStatus.Reopened:
                return next == PolicyAckSnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PolicyAckSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts - Rate, 2);
    }

    public PolicyAckSnapshot Copy()
    {
        return new PolicyAckSnapshot(Code, Label, Attributes, Rate, Attempts, Category, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
