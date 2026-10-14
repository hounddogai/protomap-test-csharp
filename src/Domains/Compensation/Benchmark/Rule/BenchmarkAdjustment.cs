namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark adjustment is in its life cycle.</summary>
public enum BenchmarkAdjustmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A benchmark adjustment. Its code is unique within a tenant.</summary>
public sealed class BenchmarkAdjustment(string code, string label, List<string> tags, string externalRef, int batchSize, string skill, BenchmarkAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public string ExternalRef { get; set; } = externalRef;
    public int BatchSize { get; set; } = batchSize;
    public string Skill { get; set; } = skill;
    public BenchmarkAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark adjustment in status created.</summary>
    public static BenchmarkAdjustment NewBenchmarkAdjustment(string code, string label)
    {
        return new BenchmarkAdjustment(
            code,
            label,
            new List<string>(),
            "",
            0,
            "",
            BenchmarkAdjustmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkAdjustmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkAdjustmentStatus next)
    {
        switch (Status)
        {
            case BenchmarkAdjustmentStatus.Created:
                switch (next)
                {
                    case BenchmarkAdjustmentStatus.Published:
                        return true;
                }
                return false;
            case BenchmarkAdjustmentStatus.Validated:
                switch (next)
                {
                    case BenchmarkAdjustmentStatus.Published:
                    case BenchmarkAdjustmentStatus.Retired:
                        return true;
                }
                return false;
            case BenchmarkAdjustmentStatus.Published:
                return next == BenchmarkAdjustmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BenchmarkAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public BenchmarkAdjustment Copy()
    {
        return new BenchmarkAdjustment(Code, Label, Tags, ExternalRef, BatchSize, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
