namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan target is in its life cycle.</summary>
public enum SuccessionPlanTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A succession plan target as the succession plan team keeps it.</summary>
public sealed class SuccessionPlanTarget(string code, string name, bool approved, double threshold, int updatedAt, string salaryBand, string costCenter, string team, int limit, double score, SuccessionPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public int UpdatedAt { get; set; } = updatedAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public string Team { get; set; } = team;
    public int Limit { get; set; } = limit;
    public double Score { get; set; } = score;
    public SuccessionPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan target in status idle.</summary>
    public static SuccessionPlanTarget NewSuccessionPlanTarget(string code, string name)
    {
        return new SuccessionPlanTarget(
            code,
            name,
            false,
            0.0,
            0,
            "",
            "",
            "",
            0,
            0.0,
            SuccessionPlanTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanTargetStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanTargetStatus.Idle:
                return next == SuccessionPlanTargetStatus.Ready;
            case SuccessionPlanTargetStatus.Warming:
                return next == SuccessionPlanTargetStatus.Draining;
            case SuccessionPlanTargetStatus.Ready:
                return next == SuccessionPlanTargetStatus.Draining || next == SuccessionPlanTargetStatus.Stopped;
            case SuccessionPlanTargetStatus.Draining:
                return next == SuccessionPlanTargetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"succession plan target"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Threshold * Score, 2);
    }

    public SuccessionPlanTarget Copy()
    {
        return new SuccessionPlanTarget(
            Code,
            Name,
            Approved,
            Threshold,
            UpdatedAt,
            SalaryBand,
            CostCenter,
            Team,
            Limit,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
