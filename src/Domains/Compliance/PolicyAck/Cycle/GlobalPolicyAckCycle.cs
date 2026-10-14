namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global policy ack cycle is in its life cycle.</summary>
public enum GlobalPolicyAckCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a global policy ack cycle and its current status.</summary>
public sealed class GlobalPolicyAckCycle(string code, string title, int retries, string badge, string phone, int startsAt, int attempts, GlobalPolicyAckCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public string Phone { get; set; } = phone;
    public int StartsAt { get; set; } = startsAt;
    public int Attempts { get; set; } = attempts;
    public GlobalPolicyAckCycleStatus Status { get; set; } = status;

    /// <summary>Starts a global policy ack cycle in status requested.</summary>
    public static GlobalPolicyAckCycle NewGlobalPolicyAckCycle(string code, string title)
    {
        return new GlobalPolicyAckCycle(code, title, 0, "", "", 0, 0, GlobalPolicyAckCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPolicyAckCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPolicyAckCycleStatus next)
    {
        switch (Status)
        {
            case GlobalPolicyAckCycleStatus.Requested:
                switch (next)
                {
                    case GlobalPolicyAckCycleStatus.Revoked:
                        return true;
                }
                return false;
            case GlobalPolicyAckCycleStatus.Granted:
                switch (next)
                {
                    case GlobalPolicyAckCycleStatus.Revoked:
                    case GlobalPolicyAckCycleStatus.Expired:
                        return true;
                }
                return false;
            case GlobalPolicyAckCycleStatus.Revoked:
                return next == GlobalPolicyAckCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalPolicyAckCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global policy ack cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Attempts * (double)StartsAt, 2);
    }

    public GlobalPolicyAckCycle Copy()
    {
        return new GlobalPolicyAckCycle(Code, Title, Retries, Badge, Phone, StartsAt, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
