namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack baseline is in its life cycle.</summary>
public enum PolicyAckBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A policy ack baseline. Its code is unique within a tenant.</summary>
public sealed class PolicyAckBaseline(string code, string title, double discount, string segment, double cost, PolicyAckBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Segment { get; set; } = segment;
    public double Cost { get; set; } = cost;
    public PolicyAckBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack baseline in status draft.</summary>
    public static PolicyAckBaseline NewPolicyAckBaseline(string code, string title)
    {
        return new PolicyAckBaseline(code, title, 0.0, "", 0.0, PolicyAckBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckBaselineStatus next)
    {
        switch (Status)
        {
            case PolicyAckBaselineStatus.Draft:
                return next == PolicyAckBaselineStatus.Pending || next == PolicyAckBaselineStatus.Closed;
            case PolicyAckBaselineStatus.Pending:
                return next == PolicyAckBaselineStatus.Active;
            case PolicyAckBaselineStatus.Active:
                switch (next)
                {
                    case PolicyAckBaselineStatus.Suspended:
                    case PolicyAckBaselineStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAckBaselineStatus.Suspended:
                switch (next)
                {
                    case PolicyAckBaselineStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Discount * Cost, 2);
    }

    public PolicyAckBaseline Copy()
    {
        return new PolicyAckBaseline(Code, Title, Discount, Segment, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
