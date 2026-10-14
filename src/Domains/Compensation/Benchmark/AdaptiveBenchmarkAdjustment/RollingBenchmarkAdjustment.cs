namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling benchmark adjustment is in its life cycle.</summary>
public enum RollingBenchmarkAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rolling benchmark adjustment. Its code is unique within a tenant.</summary>
public sealed class RollingBenchmarkAdjustment(string code, string name, string grade, string title, int windowDays, int graceDays, string label, string externalRef, string location, int retries, double margin, RollingBenchmarkAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Location { get; set; } = location;
    public int Retries { get; set; } = retries;
    public double Margin { get; set; } = margin;
    public RollingBenchmarkAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling benchmark adjustment in status idle.</summary>
    public static RollingBenchmarkAdjustment NewRollingBenchmarkAdjustment(string code, string name)
    {
        return new RollingBenchmarkAdjustment(
            code,
            name,
            "",
            "",
            0,
            0,
            "",
            "",
            "",
            0,
            0.0,
            RollingBenchmarkAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBenchmarkAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBenchmarkAdjustmentStatus next)
    {
        switch (Status)
        {
            case RollingBenchmarkAdjustmentStatus.Idle:
                switch (next)
                {
                    case RollingBenchmarkAdjustmentStatus.Warming:
                    case RollingBenchmarkAdjustmentStatus.Ready:
                        return true;
                }
                return false;
            case RollingBenchmarkAdjustmentStatus.Warming:
                switch (next)
                {
                    case RollingBenchmarkAdjustmentStatus.Ready:
                        return true;
                }
                return false;
            case RollingBenchmarkAdjustmentStatus.Ready:
                return next == RollingBenchmarkAdjustmentStatus.Draining || next == RollingBenchmarkAdjustmentStatus.Stopped;
            case RollingBenchmarkAdjustmentStatus.Draining:
                switch (next)
                {
                    case RollingBenchmarkAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingBenchmarkAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling benchmark adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Margin + (double)WindowDays, 2);
    }

    public RollingBenchmarkAdjustment Copy()
    {
        return new RollingBenchmarkAdjustment(
            Code,
            Name,
            Grade,
            Title,
            WindowDays,
            GraceDays,
            Label,
            ExternalRef,
            Location,
            Retries,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
