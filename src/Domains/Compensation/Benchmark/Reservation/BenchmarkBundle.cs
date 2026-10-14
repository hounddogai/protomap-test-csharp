namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark bundle is in its life cycle.</summary>
public enum BenchmarkBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One benchmark bundle, with what the planners need to act on it.</summary>
public sealed class BenchmarkBundle(string code, string label, int batchSize, int sequence, int windowDays, string summary, string name, BenchmarkBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int Sequence { get; set; } = sequence;
    public int WindowDays { get; set; } = windowDays;
    public string Summary { get; set; } = summary;
    public string Name { get; set; } = name;
    public BenchmarkBundleStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark bundle in status idle.</summary>
    public static BenchmarkBundle NewBenchmarkBundle(string code, string label)
    {
        return new BenchmarkBundle(code, label, 0, 0, 0, "", "", BenchmarkBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkBundleStatus next)
    {
        switch (Status)
        {
            case BenchmarkBundleStatus.Idle:
                switch (next)
                {
                    case BenchmarkBundleStatus.Ready:
                    case BenchmarkBundleStatus.Draining:
                        return true;
                }
                return false;
            case BenchmarkBundleStatus.Warming:
                return next == BenchmarkBundleStatus.Stopped;
            case BenchmarkBundleStatus.Ready:
                switch (next)
                {
                    case BenchmarkBundleStatus.Draining:
                    case BenchmarkBundleStatus.Stopped:
                        return true;
                }
                return false;
            case BenchmarkBundleStatus.Draining:
                return next == BenchmarkBundleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BenchmarkBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Sequence - (double)BatchSize, 2);
    }

    public BenchmarkBundle Copy()
    {
        return new BenchmarkBundle(Code, Label, BatchSize, Sequence, WindowDays, Summary, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
