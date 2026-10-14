namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool quota is in its life cycle.</summary>
public enum PoolQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pool quota and its current status.</summary>
public sealed class PoolQuota(string code, string label, double maximum, double ceiling, double score, bool pinned, string salaryBand, int quantity, string ownerId, double budget, string location, PoolQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public double Ceiling { get; set; } = ceiling;
    public double Score { get; set; } = score;
    public bool Pinned { get; set; } = pinned;
    public string SalaryBand { get; set; } = salaryBand;
    public int Quantity { get; set; } = quantity;
    public string OwnerId { get; set; } = ownerId;
    public double Budget { get; set; } = budget;
    public string Location { get; set; } = location;
    public PoolQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pool quota in status idle.</summary>
    public static PoolQuota NewPoolQuota(string code, string label)
    {
        return new PoolQuota(code, label, 0.0, 0.0, 0.0, false, "", 0, "", 0.0, "", PoolQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolQuotaStatus next)
    {
        switch (Status)
        {
            case PoolQuotaStatus.Idle:
                switch (next)
                {
                    case PoolQuotaStatus.Warming:
                    case PoolQuotaStatus.Ready:
                        return true;
                }
                return false;
            case PoolQuotaStatus.Warming:
                switch (next)
                {
                    case PoolQuotaStatus.Ready:
                    case PoolQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case PoolQuotaStatus.Ready:
                switch (next)
                {
                    case PoolQuotaStatus.Draining:
                    case PoolQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case PoolQuotaStatus.Draining:
                return next == PoolQuotaStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PoolQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pool quota"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Quantity - Maximum, 2);
    }

    public PoolQuota Copy()
    {
        return new PoolQuota(
            Code,
            Label,
            Maximum,
            Ceiling,
            Score,
            Pinned,
            SalaryBand,
            Quantity,
            OwnerId,
            Budget,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
