namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled benchmark schedule is in its life cycle.</summary>
public enum ScheduledBenchmarkScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a scheduled benchmark schedule and its current status.</summary>
public sealed class ScheduledBenchmarkSchedule(string code, string label, int retries, bool archived, string badge, double amount, int createdAt, string location, string name, ScheduledBenchmarkScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public bool Archived { get; set; } = archived;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public int CreatedAt { get; set; } = createdAt;
    public string Location { get; set; } = location;
    public string Name { get; set; } = name;
    public ScheduledBenchmarkScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled benchmark schedule in status new.</summary>
    public static ScheduledBenchmarkSchedule NewScheduledBenchmarkSchedule(string code, string label)
    {
        return new ScheduledBenchmarkSchedule(
            code,
            label,
            0,
            false,
            "",
            0.0,
            0,
            "",
            "",
            ScheduledBenchmarkScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBenchmarkScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBenchmarkScheduleStatus next)
    {
        switch (Status)
        {
            case ScheduledBenchmarkScheduleStatus.New:
                return next == ScheduledBenchmarkScheduleStatus.Queued || next == ScheduledBenchmarkScheduleStatus.Running;
            case ScheduledBenchmarkScheduleStatus.Queued:
                switch (next)
                {
                    case ScheduledBenchmarkScheduleStatus.Running:
                    case ScheduledBenchmarkScheduleStatus.Done:
                        return true;
                }
                return false;
            case ScheduledBenchmarkScheduleStatus.Running:
                return next == ScheduledBenchmarkScheduleStatus.Done || next == ScheduledBenchmarkScheduleStatus.Failed;
            case ScheduledBenchmarkScheduleStatus.Done:
                return next == ScheduledBenchmarkScheduleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledBenchmarkScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled benchmark schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Retries * (double)CreatedAt, 2);
    }

    public ScheduledBenchmarkSchedule Copy()
    {
        return new ScheduledBenchmarkSchedule(
            Code,
            Label,
            Retries,
            Archived,
            Badge,
            Amount,
            CreatedAt,
            Location,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
