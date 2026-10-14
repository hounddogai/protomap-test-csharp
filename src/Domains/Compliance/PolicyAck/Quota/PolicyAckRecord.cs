namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack record is in its life cycle.</summary>
public enum PolicyAckRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A policy ack record. Its code is unique within a tenant.</summary>
public sealed class PolicyAckRecord(string code, string name, double minimum, int graceDays, string skill, int endsAt, string segment, string source, string description, int retries, double maximum, PolicyAckRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public int GraceDays { get; set; } = graceDays;
    public string Skill { get; set; } = skill;
    public int EndsAt { get; set; } = endsAt;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public string Description { get; set; } = description;
    public int Retries { get; set; } = retries;
    public double Maximum { get; set; } = maximum;
    public PolicyAckRecordStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack record in status proposed.</summary>
    public static PolicyAckRecord NewPolicyAckRecord(string code, string name)
    {
        return new PolicyAckRecord(code, name, 0.0, 0, "", 0, "", "", "", 0, 0.0, PolicyAckRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckRecordStatus next)
    {
        switch (Status)
        {
            case PolicyAckRecordStatus.Proposed:
                return next == PolicyAckRecordStatus.Rejected;
            case PolicyAckRecordStatus.Approved:
                return next == PolicyAckRecordStatus.Rejected || next == PolicyAckRecordStatus.Withdrawn;
            case PolicyAckRecordStatus.Rejected:
                return next == PolicyAckRecordStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyAckRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)EndsAt * Maximum, 2);
    }

    public PolicyAckRecord Copy()
    {
        return new PolicyAckRecord(
            Code,
            Name,
            Minimum,
            GraceDays,
            Skill,
            EndsAt,
            Segment,
            Source,
            Description,
            Retries,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
