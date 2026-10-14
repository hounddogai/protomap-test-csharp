namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual benchmark metric is in its life cycle.</summary>
public enum ManualBenchmarkMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One manual benchmark metric, with what the planners need to act on it.</summary>
public sealed class ManualBenchmarkMetric(string code, string label, int attempts, bool active, int startsAt, int createdAt, int quantity, ManualBenchmarkMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public bool Active { get; set; } = active;
    public int StartsAt { get; set; } = startsAt;
    public int CreatedAt { get; set; } = createdAt;
    public int Quantity { get; set; } = quantity;
    public ManualBenchmarkMetricStatus Status { get; set; } = status;

    /// <summary>Starts a manual benchmark metric in status proposed.</summary>
    public static ManualBenchmarkMetric NewManualBenchmarkMetric(string code, string label)
    {
        return new ManualBenchmarkMetric(code, label, 0, false, 0, 0, 0, ManualBenchmarkMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBenchmarkMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBenchmarkMetricStatus next)
    {
        switch (Status)
        {
            case ManualBenchmarkMetricStatus.Proposed:
                return next == ManualBenchmarkMetricStatus.Approved || next == ManualBenchmarkMetricStatus.Withdrawn;
            case ManualBenchmarkMetricStatus.Approved:
                switch (next)
                {
                    case ManualBenchmarkMetricStatus.Rejected:
                        return true;
                }
                return false;
            case ManualBenchmarkMetricStatus.Rejected:
                return next == ManualBenchmarkMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualBenchmarkMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual benchmark metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)CreatedAt + (double)Attempts, 2);
    }

    public ManualBenchmarkMetric Copy()
    {
        return new ManualBenchmarkMetric(Code, Label, Attempts, Active, StartsAt, CreatedAt, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
