namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default town hall plan calculation.</summary>
public interface IDefaultTownHallPlanModifier
{
    string Describe();
    double Adjust(DefaultTownHallPlan item, double valueValue);
}

public sealed record CountryDefaultTownHallPlanAdjuster(Dictionary<string, double> Factors) : IDefaultTownHallPlanModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DefaultTownHallPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDefaultTownHallPlanAdjuster(double Minimum) : IDefaultTownHallPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultTownHallPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDefaultTownHallPlanAdjuster(List<double> Bounds, List<double> Rates) : IDefaultTownHallPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultTownHallPlan item, double valueValue)
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

/// <summary>Runs the default town hall plan steps in order and keeps a trace of each.</summary>
public sealed class DefaultTownHallPlanEngine(List<IDefaultTownHallPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultTownHallPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultTownHallPlanEngine DefaultDefaultTownHallPlanEngine()
    {
        return new DefaultTownHallPlanEngine(
            new List<IDefaultTownHallPlanModifier>
            {
                new CountryDefaultTownHallPlanAdjuster(new Dictionary<string, double> { ["online"] = 0.98 }),
                new MinimumDefaultTownHallPlanAdjuster(28.6),
                new TieredDefaultTownHallPlanAdjuster(
                    new List<double> { 2254.0, 9017.0, 9692.0 },
                    new List<double> { 0.94, 1.0, 1.12 }),
            },
            new List<string>(),
            2.8,
            72243.1);
    }

    public double Evaluate(DefaultTownHallPlan item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultTownHallPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
