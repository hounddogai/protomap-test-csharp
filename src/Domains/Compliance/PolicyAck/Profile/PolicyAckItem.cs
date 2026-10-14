namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack item is in its life cycle.</summary>
public enum PolicyAckItemStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One policy ack item, with what the planners need to act on it.</summary>
public sealed class PolicyAckItem(string code, string label, int sequence, double rate, double discount, PolicyAckItemStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public double Discount { get; set; } = discount;
    public PolicyAckItemStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack item in status requested.</summary>
    public static PolicyAckItem NewPolicyAckItem(string code, string label)
    {
        return new PolicyAckItem(code, label, 0, 0.0, 0.0, PolicyAckItemStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckItemStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckItemStatus next)
    {
        switch (Status)
        {
            case PolicyAckItemStatus.Requested:
                return next == PolicyAckItemStatus.Granted || next == PolicyAckItemStatus.Revoked;
            case PolicyAckItemStatus.Granted:
                switch (next)
                {
                    case PolicyAckItemStatus.Revoked:
                    case PolicyAckItemStatus.Expired:
                        return true;
                }
                return false;
            case PolicyAckItemStatus.Revoked:
                return next == PolicyAckItemStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PolicyAckItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Sequence - Rate, 2);
    }

    public PolicyAckItem Copy()
    {
        return new PolicyAckItem(Code, Label, Sequence, Rate, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
