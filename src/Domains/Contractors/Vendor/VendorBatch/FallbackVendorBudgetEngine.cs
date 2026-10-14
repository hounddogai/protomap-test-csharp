namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback vendor budget calculation.</summary>
public interface IFallbackVendorBudgetFactor
{
    string Describe();
    double Adjust(FallbackVendorBudget item, double valueValue);
}

public sealed record ChannelFallbackVendorBudgetAdjuster(Dictionary<string, double> Factors) : IFallbackVendorBudgetFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(FallbackVendorBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentFallbackVendorBudgetAdjuster(double Percent) : IFallbackVendorBudgetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackVendorBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackVendorBudgetGraceDaysOffsetAdjuster(double Weight) : IFallbackVendorBudgetFactor
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(FallbackVendorBudget item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record RoundingFallbackVendorBudgetAdjuster(double Step) : IFallbackVendorBudgetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackVendorBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback vendor budget steps in order and keeps a trace of each.</summary>
public sealed class FallbackVendorBudgetEngine(List<IFallbackVendorBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackVendorBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackVendorBudgetEngine DefaultFallbackVendorBudgetEngine()
    {
        return new FallbackVendorBudgetEngine(
            new List<IFallbackVendorBudgetFactor>
            {
                new ChannelFallbackVendorBudgetAdjuster(new Dictionary<string, double> { ["online"] = 1.37 }),
                new PercentFallbackVendorBudgetAdjuster(-9.8),
                new FallbackVendorBudgetGraceDaysOffsetAdjuster(0.04),
                new RoundingFallbackVendorBudgetAdjuster(0.05),
            },
            new List<string>(),
            3.6,
            44429.3);
    }

    public double Price(FallbackVendorBudget item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackVendorBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
