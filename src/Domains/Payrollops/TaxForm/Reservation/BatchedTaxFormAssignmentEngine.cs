namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched tax form assignment calculation.</summary>
public interface IBatchedTaxFormAssignmentStep
{
    string Describe();
    double Adjust(BatchedTaxFormAssignment item, double valueValue);
}

public sealed record BatchedTaxFormAssignmentCreatedAtOffsetAdjuster(double Weight) : IBatchedTaxFormAssignmentStep
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(BatchedTaxFormAssignment item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record RoundingBatchedTaxFormAssignmentAdjuster(double Step) : IBatchedTaxFormAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedTaxFormAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleBatchedTaxFormAssignmentAdjuster(Dictionary<string, double> Factors) : IBatchedTaxFormAssignmentStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BatchedTaxFormAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBatchedTaxFormAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IBatchedTaxFormAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedTaxFormAssignment item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record FlaggedBatchedTaxFormAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IBatchedTaxFormAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedTaxFormAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched tax form assignment steps in order and keeps a trace of each.</summary>
public sealed class BatchedTaxFormAssignmentEngine(List<IBatchedTaxFormAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedTaxFormAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedTaxFormAssignmentEngine DefaultBatchedTaxFormAssignmentEngine(Registry flags)
    {
        return new BatchedTaxFormAssignmentEngine(
            new List<IBatchedTaxFormAssignmentStep>
            {
                new BatchedTaxFormAssignmentCreatedAtOffsetAdjuster(0.58),
                new RoundingBatchedTaxFormAssignmentAdjuster(5.0),
                new TitleBatchedTaxFormAssignmentAdjuster(new Dictionary<string, double> { ["north"] = 1.19 }),
                new TieredBatchedTaxFormAssignmentAdjuster(
                    new List<double> { 2119.0, 6417.0, 8234.0, 8637.0 },
                    new List<double> { 0.94, 1.19, 1.14, 1.19 }),
                new FlaggedBatchedTaxFormAssignmentAdjuster(flags, "batched-tax-form-assignment-scheduled", 8.9),
            },
            new List<string>(),
            3.8,
            37900.1);
    }

    public double Run(BatchedTaxFormAssignment item)
    {
        var current = item.EffectiveValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BatchedTaxFormAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
