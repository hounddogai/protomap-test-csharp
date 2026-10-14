namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback return plan bundle is in its life cycle.</summary>
public enum FallbackReturnPlanBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One fallback return plan bundle, with what the planners need to act on it.</summary>
public sealed class FallbackReturnPlanBundle(string code, string label, int attempts, double score, int createdAt, int endsAt, int version, int durationMinutes, int expiresAt, FallbackReturnPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public double Score { get; set; } = score;
    public int CreatedAt { get; set; } = createdAt;
    public int EndsAt { get; set; } = endsAt;
    public int Version { get; set; } = version;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int ExpiresAt { get; set; } = expiresAt;
    public FallbackReturnPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback return plan bundle in status proposed.</summary>
    public static FallbackReturnPlanBundle NewFallbackReturnPlanBundle(string code, string label)
    {
        return new FallbackReturnPlanBundle(
            code,
            label,
            0,
            0.0,
            0,
            0,
            0,
            0,
            0,
            FallbackReturnPlanBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackReturnPlanBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackReturnPlanBundleStatus next)
    {
        switch (Status)
        {
            case FallbackReturnPlanBundleStatus.Proposed:
                switch (next)
                {
                    case FallbackReturnPlanBundleStatus.Approved:
                    case FallbackReturnPlanBundleStatus.Rejected:
                        return true;
                }
                return false;
            case FallbackReturnPlanBundleStatus.Approved:
                switch (next)
                {
                    case FallbackReturnPlanBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackReturnPlanBundleStatus.Rejected:
                return next == FallbackReturnPlanBundleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackReturnPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback return plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)DurationMinutes - (double)CreatedAt, 2);
    }

    public FallbackReturnPlanBundle Copy()
    {
        return new FallbackReturnPlanBundle(
            Code,
            Label,
            Attempts,
            Score,
            CreatedAt,
            EndsAt,
            Version,
            DurationMinutes,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
