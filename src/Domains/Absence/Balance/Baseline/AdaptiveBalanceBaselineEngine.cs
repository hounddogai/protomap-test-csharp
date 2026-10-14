namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive balance baseline calculation.</summary>
public interface IAdaptiveBalanceBaselineFactor
{
    string Describe();
    double Adjust(AdaptiveBalanceBaseline item, double valueValue);
}

public sealed record RoundingAdaptiveBalanceBaselineAdjuster(double Step) : IAdaptiveBalanceBaselineFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveBalanceBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAdaptiveBalanceBaselineAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveBalanceBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveBalanceBaseline item, double valueValue)
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

public sealed record AdaptiveBalanceBaselineWindowDaysOffsetAdjuster(double Weight) : IAdaptiveBalanceBaselineFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(AdaptiveBalanceBaseline item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record SourceAdaptiveBalanceBaselineAdjuster(Dictionary<string, double> Factors) : IAdaptiveBalanceBaselineFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AdaptiveBalanceBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive balance baseline steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveBalanceBaselineEngine(List<IAdaptiveBalanceBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveBalanceBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveBalanceBaselineEngine DefaultAdaptiveBalanceBaselineEngine()
    {
        return new AdaptiveBalanceBaselineEngine(
            new List<IAdaptiveBalanceBaselineFactor>
            {
                new RoundingAdaptiveBalanceBaselineAdjuster(0.5),
                new TieredAdaptiveBalanceBaselineAdjuster(
                    new List<double> { 2140.0, 3877.0, 8102.0, 9657.0 },
                    new List<double> { 0.96, 1.05, 0.98, 1.04 }),
                new AdaptiveBalanceBaselineWindowDaysOffsetAdjuster(0.77),
                new SourceAdaptiveBalanceBaselineAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.72, ["west"] = 1.03 }),
            },
            new List<string>(),
            3.5,
            40773.9);
    }

    public double Evaluate(AdaptiveBalanceBaseline item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveBalanceBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
