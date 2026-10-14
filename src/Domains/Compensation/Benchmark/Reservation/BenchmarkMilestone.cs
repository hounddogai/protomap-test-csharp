namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark milestone is in its life cycle.</summary>
public enum BenchmarkMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A benchmark milestone as the benchmark team keeps it.</summary>
public sealed class BenchmarkMilestone(string code, string label, string skill, double score, int batchSize, bool visible, double discount, string managerId, bool approved, string location, BenchmarkMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Score { get; set; } = score;
    public int BatchSize { get; set; } = batchSize;
    public bool Visible { get; set; } = visible;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public BenchmarkMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark milestone in status planned.</summary>
    public static BenchmarkMilestone NewBenchmarkMilestone(string code, string label)
    {
        return new BenchmarkMilestone(
            code,
            label,
            "",
            0.0,
            0,
            false,
            0.0,
            "",
            false,
            "",
            BenchmarkMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkMilestoneStatus next)
    {
        switch (Status)
        {
            case BenchmarkMilestoneStatus.Planned:
                switch (next)
                {
                    case BenchmarkMilestoneStatus.InProgress:
                        return true;
                }
                return false;
            case BenchmarkMilestoneStatus.Scheduled:
                return next == BenchmarkMilestoneStatus.Completed;
            case BenchmarkMilestoneStatus.InProgress:
                return next == BenchmarkMilestoneStatus.Completed || next == BenchmarkMilestoneStatus.Cancelled;
            case BenchmarkMilestoneStatus.Completed:
                return next == BenchmarkMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BenchmarkMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Discount - Score, 2);
    }

    public BenchmarkMilestone Copy()
    {
        return new BenchmarkMilestone(
            Code,
            Label,
            Skill,
            Score,
            BatchSize,
            Visible,
            Discount,
            ManagerId,
            Approved,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
