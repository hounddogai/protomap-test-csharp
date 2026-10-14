namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack adjustment is in its life cycle.</summary>
public enum PolicyAckAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One policy ack adjustment, with what the planners need to act on it.</summary>
public sealed class PolicyAckAdjustment(string code, string title, double cost, int limit, string currency, string ownerId, int graceDays, double weight, PolicyAckAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int Limit { get; set; } = limit;
    public string Currency { get; set; } = currency;
    public string OwnerId { get; set; } = ownerId;
    public int GraceDays { get; set; } = graceDays;
    public double Weight { get; set; } = weight;
    public PolicyAckAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack adjustment in status idle.</summary>
    public static PolicyAckAdjustment NewPolicyAckAdjustment(string code, string title)
    {
        return new PolicyAckAdjustment(code, title, 0.0, 0, "", "", 0, 0.0, PolicyAckAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckAdjustmentStatus next)
    {
        switch (Status)
        {
            case PolicyAckAdjustmentStatus.Idle:
                switch (next)
                {
                    case PolicyAckAdjustmentStatus.Warming:
                        return true;
                }
                return false;
            case PolicyAckAdjustmentStatus.Warming:
                return next == PolicyAckAdjustmentStatus.Draining;
            case PolicyAckAdjustmentStatus.Ready:
                switch (next)
                {
                    case PolicyAckAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case PolicyAckAdjustmentStatus.Draining:
                switch (next)
                {
                    case PolicyAckAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)GraceDays * Cost, 2);
    }

    public PolicyAckAdjustment Copy()
    {
        return new PolicyAckAdjustment(Code, Title, Cost, Limit, Currency, OwnerId, GraceDays, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
