namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan policy is in its life cycle.</summary>
public enum SuccessionPlanPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A succession plan policy. Its code is unique within a tenant.</summary>
public sealed class SuccessionPlanPolicy(string code, string label, string costCenter, double threshold, int endsAt, int windowDays, string region, string badge, string segment, SuccessionPlanPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public double Threshold { get; set; } = threshold;
    public int EndsAt { get; set; } = endsAt;
    public int WindowDays { get; set; } = windowDays;
    public string Region { get; set; } = region;
    public string Badge { get; set; } = badge;
    public string Segment { get; set; } = segment;
    public SuccessionPlanPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan policy in status idle.</summary>
    public static SuccessionPlanPolicy NewSuccessionPlanPolicy(string code, string label)
    {
        return new SuccessionPlanPolicy(code, label, "", 0.0, 0, 0, "", "", "", SuccessionPlanPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanPolicyStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanPolicyStatus.Idle:
                return next == SuccessionPlanPolicyStatus.Warming;
            case SuccessionPlanPolicyStatus.Warming:
                return next == SuccessionPlanPolicyStatus.Draining;
            case SuccessionPlanPolicyStatus.Ready:
                return next == SuccessionPlanPolicyStatus.Draining;
            case SuccessionPlanPolicyStatus.Draining:
                return next == SuccessionPlanPolicyStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt * (double)WindowDays, 2);
    }

    public SuccessionPlanPolicy Copy()
    {
        return new SuccessionPlanPolicy(
            Code,
            Label,
            CostCenter,
            Threshold,
            EndsAt,
            WindowDays,
            Region,
            Badge,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
