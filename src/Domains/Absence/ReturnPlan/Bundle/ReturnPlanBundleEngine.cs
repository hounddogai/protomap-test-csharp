namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan bundle calculation.</summary>
public interface IReturnPlanBundleFactor
{
    string Describe();
    double Adjust(ReturnPlanBundle item, double valueValue);
}

public sealed record PercentReturnPlanBundleAdjuster(double Percent) : IReturnPlanBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ReturnPlanBundleMinimumOffsetAdjuster(double Weight) : IReturnPlanBundleFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(ReturnPlanBundle item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingReturnPlanBundleAdjuster(double Step) : IReturnPlanBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapReturnPlanBundleAdjuster(double Cap) : IReturnPlanBundleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredReturnPlanBundleAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanBundle item, double valueValue)
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

/// <summary>Runs the return plan bundle steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanBundleEngine(List<IReturnPlanBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanBundleEngine DefaultReturnPlanBundleEngine()
    {
        return new ReturnPlanBundleEngine(
            new List<IReturnPlanBundleFactor>
            {
                new PercentReturnPlanBundleAdjuster(15.7),
                new ReturnPlanBundleMinimumOffsetAdjuster(0.72),
                new RoundingReturnPlanBundleAdjuster(0.25),
                new CapReturnPlanBundleAdjuster(33282.0),
                new TieredReturnPlanBundleAdjuster(
                    new List<double> { 577.0, 1384.0, 4530.0, 6448.0 },
                    new List<double> { 1.12, 1.15, 1.16, 0.98 }),
            },
            new List<string>(),
            0.3,
            10055.9);
    }

    public double Price(ReturnPlanBundle item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
