namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback succession plan quota is in its life cycle.</summary>
public enum FallbackSuccessionPlanQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a fallback succession plan quota and its current status.</summary>
public sealed class FallbackSuccessionPlanQuota(string code, string name, string notes, int capacity, bool pinned, string salaryBand, int endsAt, bool approved, FallbackSuccessionPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public int Capacity { get; set; } = capacity;
    public bool Pinned { get; set; } = pinned;
    public string SalaryBand { get; set; } = salaryBand;
    public int EndsAt { get; set; } = endsAt;
    public bool Approved { get; set; } = approved;
    public FallbackSuccessionPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a fallback succession plan quota in status idle.</summary>
    public static FallbackSuccessionPlanQuota NewFallbackSuccessionPlanQuota(string code, string name)
    {
        return new FallbackSuccessionPlanQuota(
            code,
            name,
            "",
            0,
            false,
            "",
            0,
            false,
            FallbackSuccessionPlanQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSuccessionPlanQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSuccessionPlanQuotaStatus next)
    {
        switch (Status)
        {
            case FallbackSuccessionPlanQuotaStatus.Idle:
                switch (next)
                {
                    case FallbackSuccessionPlanQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackSuccessionPlanQuotaStatus.Warming:
                switch (next)
                {
                    case FallbackSuccessionPlanQuotaStatus.Ready:
                    case FallbackSuccessionPlanQuotaStatus.Draining:
                        return true;
                }
                return false;
            case FallbackSuccessionPlanQuotaStatus.Ready:
                return next == FallbackSuccessionPlanQuotaStatus.Draining;
            case FallbackSuccessionPlanQuotaStatus.Draining:
                return next == FallbackSuccessionPlanQuotaStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackSuccessionPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback succession plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback succession plan quota"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity * (double)EndsAt, 2);
    }

    public FallbackSuccessionPlanQuota Copy()
    {
        return new FallbackSuccessionPlanQuota(
            Code,
            Name,
            Notes,
            Capacity,
            Pinned,
            SalaryBand,
            EndsAt,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
