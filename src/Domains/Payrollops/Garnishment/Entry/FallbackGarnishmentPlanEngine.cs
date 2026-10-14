namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback garnishment plan calculation.</summary>
public interface IFallbackGarnishmentPlanFactor
{
    string Describe();
    double Adjust(FallbackGarnishmentPlan item, double valueValue);
}

public sealed record TieredFallbackGarnishmentPlanAdjuster(List<double> Bounds, List<double> Rates) : IFallbackGarnishmentPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackGarnishmentPlan item, double valueValue)
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

public sealed record MinimumFallbackGarnishmentPlanAdjuster(double Minimum) : IFallbackGarnishmentPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackGarnishmentPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleFallbackGarnishmentPlanAdjuster(Dictionary<string, double> Factors) : IFallbackGarnishmentPlanFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FallbackGarnishmentPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback garnishment plan steps in order and keeps a trace of each.</summary>
public sealed class FallbackGarnishmentPlanEngine(List<IFallbackGarnishmentPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackGarnishmentPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackGarnishmentPlanEngine DefaultFallbackGarnishmentPlanEngine()
    {
        return new FallbackGarnishmentPlanEngine(
            new List<IFallbackGarnishmentPlanFactor>
            {
                new TieredFallbackGarnishmentPlanAdjuster(
                    new List<double> { 3641.0, 7235.0 },
                    new List<double> { 1.2, 0.99 }),
                new MinimumFallbackGarnishmentPlanAdjuster(30.7),
                new TitleFallbackGarnishmentPlanAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.18, ["central"] = 1.35, ["west"] = 1.1 }),
            },
            new List<string>(),
            0.3,
            31483.1);
    }

    public double Price(FallbackGarnishmentPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackGarnishmentPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
