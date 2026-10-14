namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark target is in its life cycle.</summary>
public enum BenchmarkTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a benchmark target and its current status.</summary>
public sealed class BenchmarkTarget(string code, string name, int durationMinutes, string checksum, string locale, int retries, bool pinned, double margin, int attempts, int revision, int sequence, BenchmarkTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Checksum { get; set; } = checksum;
    public string Locale { get; set; } = locale;
    public int Retries { get; set; } = retries;
    public bool Pinned { get; set; } = pinned;
    public double Margin { get; set; } = margin;
    public int Attempts { get; set; } = attempts;
    public int Revision { get; set; } = revision;
    public int Sequence { get; set; } = sequence;
    public BenchmarkTargetStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark target in status idle.</summary>
    public static BenchmarkTarget NewBenchmarkTarget(string code, string name)
    {
        return new BenchmarkTarget(code, name, 0, "", "", 0, false, 0.0, 0, 0, 0, BenchmarkTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkTargetStatus next)
    {
        switch (Status)
        {
            case BenchmarkTargetStatus.Idle:
                switch (next)
                {
                    case BenchmarkTargetStatus.Draining:
                        return true;
                }
                return false;
            case BenchmarkTargetStatus.Warming:
                return next == BenchmarkTargetStatus.Ready || next == BenchmarkTargetStatus.Draining;
            case BenchmarkTargetStatus.Ready:
                switch (next)
                {
                    case BenchmarkTargetStatus.Draining:
                        return true;
                }
                return false;
            case BenchmarkTargetStatus.Draining:
                return next == BenchmarkTargetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BenchmarkTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"benchmark target"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)DurationMinutes + (double)Attempts, 2);
    }

    public BenchmarkTarget Copy()
    {
        return new BenchmarkTarget(
            Code,
            Name,
            DurationMinutes,
            Checksum,
            Locale,
            Retries,
            Pinned,
            Margin,
            Attempts,
            Revision,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
