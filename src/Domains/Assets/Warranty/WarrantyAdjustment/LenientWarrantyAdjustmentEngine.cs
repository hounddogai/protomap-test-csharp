namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient warranty adjustment calculation.</summary>
public interface ILenientWarrantyAdjustmentStep
{
    string Describe();
    double Adjust(LenientWarrantyAdjustment item, double valueValue);
}

public sealed record LenientWarrantyAdjustmentAmountOffsetAdjuster(double Weight) : ILenientWarrantyAdjustmentStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LenientWarrantyAdjustment item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentLenientWarrantyAdjustmentAdjuster(double Percent) : ILenientWarrantyAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientWarrantyAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientWarrantyAdjustmentAdjuster(double Step) : ILenientWarrantyAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientWarrantyAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLenientWarrantyAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ILenientWarrantyAdjustmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientWarrantyAdjustment item, double valueValue)
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

public sealed record TitleLenientWarrantyAdjustmentAdjuster(Dictionary<string, double> Factors) : ILenientWarrantyAdjustmentStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientWarrantyAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientWarrantyAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ILenientWarrantyAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientWarrantyAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient warranty adjustment steps in order and keeps a trace of each.</summary>
public sealed class LenientWarrantyAdjustmentEngine(List<ILenientWarrantyAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientWarrantyAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientWarrantyAdjustmentEngine DefaultLenientWarrantyAdjustmentEngine(Registry flags)
    {
        return new LenientWarrantyAdjustmentEngine(
            new List<ILenientWarrantyAdjustmentStep>
            {
                new LenientWarrantyAdjustmentAmountOffsetAdjuster(1.63),
                new PercentLenientWarrantyAdjustmentAdjuster(10.5),
                new RoundingLenientWarrantyAdjustmentAdjuster(1.0),
                new TieredLenientWarrantyAdjustmentAdjuster(
                    new List<double> { 786.0, 3365.0, 7418.0 },
                    new List<double> { 0.98, 0.99, 1.02 }),
                new TitleLenientWarrantyAdjustmentAdjuster(new Dictionary<string, double> { ["east"] = 0.86 }),
                new FlaggedLenientWarrantyAdjustmentAdjuster(flags, "lenient-warranty-adjustment-batched", 5.9),
            },
            new List<string>(),
            3.2,
            2593.0);
    }

    public double Evaluate(LenientWarrantyAdjustment item)
    {
        var current = item.EffectiveTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LenientWarrantyAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
