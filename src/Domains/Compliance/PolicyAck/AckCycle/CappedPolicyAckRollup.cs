namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped policy ack rollup is in its life cycle.</summary>
public enum CappedPolicyAckRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One capped policy ack rollup, with what the planners need to act on it.</summary>
public sealed class CappedPolicyAckRollup(string code, string title, double maximum, string segment, double minimum, string summary, CappedPolicyAckRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Segment { get; set; } = segment;
    public double Minimum { get; set; } = minimum;
    public string Summary { get; set; } = summary;
    public CappedPolicyAckRollupStatus Status { get; set; } = status;

    /// <summary>Starts a capped policy ack rollup in status created.</summary>
    public static CappedPolicyAckRollup NewCappedPolicyAckRollup(string code, string title)
    {
        return new CappedPolicyAckRollup(code, title, 0.0, "", 0.0, "", CappedPolicyAckRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPolicyAckRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPolicyAckRollupStatus next)
    {
        switch (Status)
        {
            case CappedPolicyAckRollupStatus.Created:
                return next == CappedPolicyAckRollupStatus.Published || next == CappedPolicyAckRollupStatus.Retired;
            case CappedPolicyAckRollupStatus.Validated:
                return next == CappedPolicyAckRollupStatus.Published || next == CappedPolicyAckRollupStatus.Retired;
            case CappedPolicyAckRollupStatus.Published:
                return next == CappedPolicyAckRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedPolicyAckRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped policy ack rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped policy ack rollup"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Maximum * Minimum, 2);
    }

    public CappedPolicyAckRollup Copy()
    {
        return new CappedPolicyAckRollup(Code, Title, Maximum, Segment, Minimum, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
