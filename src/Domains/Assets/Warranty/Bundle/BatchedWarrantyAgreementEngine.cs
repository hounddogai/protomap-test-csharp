namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched warranty agreement calculation.</summary>
public interface IBatchedWarrantyAgreementStep
{
    string Describe();
    double Adjust(BatchedWarrantyAgreement item, double valueValue);
}

public sealed record MinimumBatchedWarrantyAgreementAdjuster(double Minimum) : IBatchedWarrantyAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedWarrantyAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedBatchedWarrantyAgreementAdjuster(Registry Flags, string Flag, double Percent) : IBatchedWarrantyAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedWarrantyAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBatchedWarrantyAgreementAdjuster(double Step) : IBatchedWarrantyAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedWarrantyAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBatchedWarrantyAgreementAdjuster(List<double> Bounds, List<double> Rates) : IBatchedWarrantyAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedWarrantyAgreement item, double valueValue)
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

/// <summary>Runs the batched warranty agreement steps in order and keeps a trace of each.</summary>
public sealed class BatchedWarrantyAgreementEngine(List<IBatchedWarrantyAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedWarrantyAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedWarrantyAgreementEngine DefaultBatchedWarrantyAgreementEngine(Registry flags)
    {
        return new BatchedWarrantyAgreementEngine(
            new List<IBatchedWarrantyAgreementStep>
            {
                new MinimumBatchedWarrantyAgreementAdjuster(14.9),
                new FlaggedBatchedWarrantyAgreementAdjuster(flags, "batched-warranty-agreement-pending", 9.4),
                new RoundingBatchedWarrantyAgreementAdjuster(0.25),
                new TieredBatchedWarrantyAgreementAdjuster(
                    new List<double> { 652.0, 3172.0, 9249.0 },
                    new List<double> { 0.8, 1.08, 0.99 }),
            },
            new List<string>(),
            2.3,
            72903.8);
    }

    public double Evaluate(BatchedWarrantyAgreement item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedWarrantyAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
