namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark threshold is in its life cycle.</summary>
public enum BenchmarkThresholdStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A benchmark threshold as the benchmark team keeps it.</summary>
public sealed class BenchmarkThreshold(string code, string label, string skill, string ownerId, string salaryBand, string notes, string summary, int quantity, BenchmarkThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string OwnerId { get; set; } = ownerId;
    public string SalaryBand { get; set; } = salaryBand;
    public string Notes { get; set; } = notes;
    public string Summary { get; set; } = summary;
    public int Quantity { get; set; } = quantity;
    public BenchmarkThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark threshold in status created.</summary>
    public static BenchmarkThreshold NewBenchmarkThreshold(string code, string label)
    {
        return new BenchmarkThreshold(code, label, "", "", "", "", "", 0, BenchmarkThresholdStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkThresholdStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkThresholdStatus next)
    {
        switch (Status)
        {
            case BenchmarkThresholdStatus.Created:
                switch (next)
                {
                    case BenchmarkThresholdStatus.Published:
                    case BenchmarkThresholdStatus.Retired:
                        return true;
                }
                return false;
            case BenchmarkThresholdStatus.Validated:
                return next == BenchmarkThresholdStatus.Published;
            case BenchmarkThresholdStatus.Published:
                switch (next)
                {
                    case BenchmarkThresholdStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public BenchmarkThreshold Copy()
    {
        return new BenchmarkThreshold(Code, Label, Skill, OwnerId, SalaryBand, Notes, Summary, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
