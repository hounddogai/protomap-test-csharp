namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark schedule is in its life cycle.</summary>
public enum BenchmarkScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a benchmark schedule and its current status.</summary>
public sealed class BenchmarkSchedule(string code, string label, int priority, string channel, double rating, string locale, BenchmarkScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Channel { get; set; } = channel;
    public double Rating { get; set; } = rating;
    public string Locale { get; set; } = locale;
    public BenchmarkScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark schedule in status new.</summary>
    public static BenchmarkSchedule NewBenchmarkSchedule(string code, string label)
    {
        return new BenchmarkSchedule(code, label, 0, "", 0.0, "", BenchmarkScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkScheduleStatus next)
    {
        switch (Status)
        {
            case BenchmarkScheduleStatus.New:
                return next == BenchmarkScheduleStatus.Failed;
            case BenchmarkScheduleStatus.Queued:
                switch (next)
                {
                    case BenchmarkScheduleStatus.Failed:
                        return true;
                }
                return false;
            case BenchmarkScheduleStatus.Running:
                return next == BenchmarkScheduleStatus.Failed;
            case BenchmarkScheduleStatus.Done:
                return next == BenchmarkScheduleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BenchmarkScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"benchmark schedule"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Rating * (double)Priority, 2);
    }

    public BenchmarkSchedule Copy()
    {
        return new BenchmarkSchedule(Code, Label, Priority, Channel, Rating, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
