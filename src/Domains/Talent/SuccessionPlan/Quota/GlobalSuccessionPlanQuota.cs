namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global succession plan quota is in its life cycle.</summary>
public enum GlobalSuccessionPlanQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A global succession plan quota as the succession plan team keeps it.</summary>
public sealed class GlobalSuccessionPlanQuota(string code, string label, string currency, string managerId, double weight, bool active, int sequence, string region, int capacity, GlobalSuccessionPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string ManagerId { get; set; } = managerId;
    public double Weight { get; set; } = weight;
    public bool Active { get; set; } = active;
    public int Sequence { get; set; } = sequence;
    public string Region { get; set; } = region;
    public int Capacity { get; set; } = capacity;
    public GlobalSuccessionPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a global succession plan quota in status idle.</summary>
    public static GlobalSuccessionPlanQuota NewGlobalSuccessionPlanQuota(string code, string label)
    {
        return new GlobalSuccessionPlanQuota(
            code,
            label,
            "",
            "",
            0.0,
            false,
            0,
            "",
            0,
            GlobalSuccessionPlanQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalSuccessionPlanQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalSuccessionPlanQuotaStatus next)
    {
        switch (Status)
        {
            case GlobalSuccessionPlanQuotaStatus.Idle:
                switch (next)
                {
                    case GlobalSuccessionPlanQuotaStatus.Warming:
                    case GlobalSuccessionPlanQuotaStatus.Ready:
                        return true;
                }
                return false;
            case GlobalSuccessionPlanQuotaStatus.Warming:
                switch (next)
                {
                    case GlobalSuccessionPlanQuotaStatus.Draining:
                    case GlobalSuccessionPlanQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalSuccessionPlanQuotaStatus.Ready:
                switch (next)
                {
                    case GlobalSuccessionPlanQuotaStatus.Draining:
                    case GlobalSuccessionPlanQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalSuccessionPlanQuotaStatus.Draining:
                switch (next)
                {
                    case GlobalSuccessionPlanQuotaStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalSuccessionPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global succession plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Capacity - Weight, 2);
    }

    public GlobalSuccessionPlanQuota Copy()
    {
        return new GlobalSuccessionPlanQuota(
            Code,
            Label,
            Currency,
            ManagerId,
            Weight,
            Active,
            Sequence,
            Region,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
