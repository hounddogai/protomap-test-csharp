namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global policy ack bundle is in its life cycle.</summary>
public enum GlobalPolicyAckBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One global policy ack bundle, with what the planners need to act on it.</summary>
public sealed class GlobalPolicyAckBundle(string code, string title, double threshold, int graceDays, string jobCode, int updatedAt, int createdAt, string costCenter, string ownerId, GlobalPolicyAckBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public int GraceDays { get; set; } = graceDays;
    public string JobCode { get; set; } = jobCode;
    public int UpdatedAt { get; set; } = updatedAt;
    public int CreatedAt { get; set; } = createdAt;
    public string CostCenter { get; set; } = costCenter;
    public string OwnerId { get; set; } = ownerId;
    public GlobalPolicyAckBundleStatus Status { get; set; } = status;

    /// <summary>Starts a global policy ack bundle in status draft.</summary>
    public static GlobalPolicyAckBundle NewGlobalPolicyAckBundle(string code, string title)
    {
        return new GlobalPolicyAckBundle(code, title, 0.0, 0, "", 0, 0, "", "", GlobalPolicyAckBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPolicyAckBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPolicyAckBundleStatus next)
    {
        switch (Status)
        {
            case GlobalPolicyAckBundleStatus.Draft:
                return next == GlobalPolicyAckBundleStatus.Pending || next == GlobalPolicyAckBundleStatus.Closed;
            case GlobalPolicyAckBundleStatus.Pending:
                return next == GlobalPolicyAckBundleStatus.Closed;
            case GlobalPolicyAckBundleStatus.Active:
                return next == GlobalPolicyAckBundleStatus.Suspended || next == GlobalPolicyAckBundleStatus.Closed;
            case GlobalPolicyAckBundleStatus.Suspended:
                return next == GlobalPolicyAckBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalPolicyAckBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global policy ack bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global policy ack bundle"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)UpdatedAt - (double)GraceDays, 2);
    }

    public GlobalPolicyAckBundle Copy()
    {
        return new GlobalPolicyAckBundle(
            Code,
            Title,
            Threshold,
            GraceDays,
            JobCode,
            UpdatedAt,
            CreatedAt,
            CostCenter,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
