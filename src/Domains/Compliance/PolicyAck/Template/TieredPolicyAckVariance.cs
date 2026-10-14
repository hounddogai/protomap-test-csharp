namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered policy ack variance is in its life cycle.</summary>
public enum TieredPolicyAckVarianceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One tiered policy ack variance, with what the planners need to act on it.</summary>
public sealed class TieredPolicyAckVariance(string code, string label, string notes, string source, int version, string location, string ownerId, Dictionary<string, string> attributes, bool archived, string managerId, string costCenter, TieredPolicyAckVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Source { get; set; } = source;
    public int Version { get; set; } = version;
    public string Location { get; set; } = location;
    public string OwnerId { get; set; } = ownerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Archived { get; set; } = archived;
    public string ManagerId { get; set; } = managerId;
    public string CostCenter { get; set; } = costCenter;
    public TieredPolicyAckVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a tiered policy ack variance in status idle.</summary>
    public static TieredPolicyAckVariance NewTieredPolicyAckVariance(string code, string label)
    {
        return new TieredPolicyAckVariance(
            code,
            label,
            "",
            "",
            0,
            "",
            "",
            new Dictionary<string, string>(),
            false,
            "",
            "",
            TieredPolicyAckVarianceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPolicyAckVarianceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPolicyAckVarianceStatus next)
    {
        switch (Status)
        {
            case TieredPolicyAckVarianceStatus.Idle:
                switch (next)
                {
                    case TieredPolicyAckVarianceStatus.Warming:
                    case TieredPolicyAckVarianceStatus.Stopped:
                        return true;
                }
                return false;
            case TieredPolicyAckVarianceStatus.Warming:
                return next == TieredPolicyAckVarianceStatus.Stopped;
            case TieredPolicyAckVarianceStatus.Ready:
                switch (next)
                {
                    case TieredPolicyAckVarianceStatus.Draining:
                    case TieredPolicyAckVarianceStatus.Stopped:
                        return true;
                }
                return false;
            case TieredPolicyAckVarianceStatus.Draining:
                switch (next)
                {
                    case TieredPolicyAckVarianceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPolicyAckVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered policy ack variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
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

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public TieredPolicyAckVariance Copy()
    {
        return new TieredPolicyAckVariance(
            Code,
            Label,
            Notes,
            Source,
            Version,
            Location,
            OwnerId,
            Attributes,
            Archived,
            ManagerId,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
