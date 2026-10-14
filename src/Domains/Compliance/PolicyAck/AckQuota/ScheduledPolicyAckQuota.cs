namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled policy ack quota is in its life cycle.</summary>
public enum ScheduledPolicyAckQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled policy ack quota. Its code is unique within a tenant.</summary>
public sealed class ScheduledPolicyAckQuota(string code, string name, double amount, int graceDays, string timezone, double discount, int priority, ScheduledPolicyAckQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int GraceDays { get; set; } = graceDays;
    public string Timezone { get; set; } = timezone;
    public double Discount { get; set; } = discount;
    public int Priority { get; set; } = priority;
    public ScheduledPolicyAckQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled policy ack quota in status planned.</summary>
    public static ScheduledPolicyAckQuota NewScheduledPolicyAckQuota(string code, string name)
    {
        return new ScheduledPolicyAckQuota(code, name, 0.0, 0, "", 0.0, 0, ScheduledPolicyAckQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPolicyAckQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPolicyAckQuotaStatus next)
    {
        switch (Status)
        {
            case ScheduledPolicyAckQuotaStatus.Planned:
                return next == ScheduledPolicyAckQuotaStatus.Scheduled;
            case ScheduledPolicyAckQuotaStatus.Scheduled:
                switch (next)
                {
                    case ScheduledPolicyAckQuotaStatus.Completed:
                    case ScheduledPolicyAckQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledPolicyAckQuotaStatus.InProgress:
                switch (next)
                {
                    case ScheduledPolicyAckQuotaStatus.Completed:
                    case ScheduledPolicyAckQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledPolicyAckQuotaStatus.Completed:
                return next == ScheduledPolicyAckQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledPolicyAckQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled policy ack quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Priority * (double)GraceDays, 2);
    }

    public ScheduledPolicyAckQuota Copy()
    {
        return new ScheduledPolicyAckQuota(Code, Name, Amount, GraceDays, Timezone, Discount, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
