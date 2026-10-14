namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack exception is in its life cycle.</summary>
public enum PolicyAckExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A policy ack exception. Its code is unique within a tenant.</summary>
public sealed class PolicyAckException(string code, string label, double fte, string notes, int createdAt, Dictionary<string, string> attributes, int batchSize, string externalRef, string description, PolicyAckExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string Notes { get; set; } = notes;
    public int CreatedAt { get; set; } = createdAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public string Description { get; set; } = description;
    public PolicyAckExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack exception in status proposed.</summary>
    public static PolicyAckException NewPolicyAckException(string code, string label)
    {
        return new PolicyAckException(
            code,
            label,
            0.0,
            "",
            0,
            new Dictionary<string, string>(),
            0,
            "",
            "",
            PolicyAckExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckExceptionStatus next)
    {
        switch (Status)
        {
            case PolicyAckExceptionStatus.Proposed:
                return next == PolicyAckExceptionStatus.Approved || next == PolicyAckExceptionStatus.Withdrawn;
            case PolicyAckExceptionStatus.Approved:
                switch (next)
                {
                    case PolicyAckExceptionStatus.Rejected:
                    case PolicyAckExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
            case PolicyAckExceptionStatus.Rejected:
                switch (next)
                {
                    case PolicyAckExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"policy ack exception"} {Code}";
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

    public double WeightedLoad()
    {
        return Math.Round((double)CreatedAt + Fte, 2);
    }

    public PolicyAckException Copy()
    {
        return new PolicyAckException(
            Code,
            Label,
            Fte,
            Notes,
            CreatedAt,
            Attributes,
            BatchSize,
            ExternalRef,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
