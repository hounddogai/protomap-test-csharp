namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending benchmark rule is in its life cycle.</summary>
public enum PendingBenchmarkRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending benchmark rule as the benchmark team keeps it.</summary>
public sealed class PendingBenchmarkRule(string code, string name, int createdAt, int graceDays, double score, double fte, double cost, string channel, double budget, double margin, PendingBenchmarkRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public int GraceDays { get; set; } = graceDays;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public double Cost { get; set; } = cost;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public double Margin { get; set; } = margin;
    public PendingBenchmarkRuleStatus Status { get; set; } = status;

    /// <summary>Starts a pending benchmark rule in status proposed.</summary>
    public static PendingBenchmarkRule NewPendingBenchmarkRule(string code, string name)
    {
        return new PendingBenchmarkRule(
            code,
            name,
            0,
            0,
            0.0,
            0.0,
            0.0,
            "",
            0.0,
            0.0,
            PendingBenchmarkRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBenchmarkRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBenchmarkRuleStatus next)
    {
        switch (Status)
        {
            case PendingBenchmarkRuleStatus.Proposed:
                return next == PendingBenchmarkRuleStatus.Approved || next == PendingBenchmarkRuleStatus.Withdrawn;
            case PendingBenchmarkRuleStatus.Approved:
                return next == PendingBenchmarkRuleStatus.Rejected || next == PendingBenchmarkRuleStatus.Withdrawn;
            case PendingBenchmarkRuleStatus.Rejected:
                return next == PendingBenchmarkRuleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PendingBenchmarkRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending benchmark rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)GraceDays - Cost, 2);
    }

    public PendingBenchmarkRule Copy()
    {
        return new PendingBenchmarkRule(
            Code,
            Name,
            CreatedAt,
            GraceDays,
            Score,
            Fte,
            Cost,
            Channel,
            Budget,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
