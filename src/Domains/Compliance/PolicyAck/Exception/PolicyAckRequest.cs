namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack request is in its life cycle.</summary>
public enum PolicyAckRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a policy ack request and its current status.</summary>
public sealed class PolicyAckRequest(string code, string title, int tenureMonths, int expiresAt, int revision, int batchSize, string segment, double ratio, string reviewer, int sequence, PolicyAckRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Revision { get; set; } = revision;
    public int BatchSize { get; set; } = batchSize;
    public string Segment { get; set; } = segment;
    public double Ratio { get; set; } = ratio;
    public string Reviewer { get; set; } = reviewer;
    public int Sequence { get; set; } = sequence;
    public PolicyAckRequestStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack request in status proposed.</summary>
    public static PolicyAckRequest NewPolicyAckRequest(string code, string title)
    {
        return new PolicyAckRequest(code, title, 0, 0, 0, 0, "", 0.0, "", 0, PolicyAckRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckRequestStatus next)
    {
        switch (Status)
        {
            case PolicyAckRequestStatus.Proposed:
                return next == PolicyAckRequestStatus.Rejected || next == PolicyAckRequestStatus.Withdrawn;
            case PolicyAckRequestStatus.Approved:
                return next == PolicyAckRequestStatus.Rejected || next == PolicyAckRequestStatus.Withdrawn;
            case PolicyAckRequestStatus.Rejected:
                switch (next)
                {
                    case PolicyAckRequestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ratio * (double)TenureMonths, 2);
    }

    public PolicyAckRequest Copy()
    {
        return new PolicyAckRequest(
            Code,
            Title,
            TenureMonths,
            ExpiresAt,
            Revision,
            BatchSize,
            Segment,
            Ratio,
            Reviewer,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
