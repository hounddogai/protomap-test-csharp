namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario budget calculation.</summary>
public interface IScenarioBudgetModifier
{
    string Describe();
    double Adjust(ScenarioBudget item, double valueValue);
}

public sealed record MinimumScenarioBudgetAdjuster(double Minimum) : IScenarioBudgetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScenarioBudgetMarginOffsetAdjuster(double Weight) : IScenarioBudgetModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(ScenarioBudget item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record SegmentScenarioBudgetAdjuster(Dictionary<string, double> Factors) : IScenarioBudgetModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ScenarioBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedScenarioBudgetAdjuster(Registry Flags, string Flag, double Percent) : IScenarioBudgetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScenarioBudgetAdjuster(List<double> Bounds, List<double> Rates) : IScenarioBudgetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioBudget item, double valueValue)
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

public sealed record RoundingScenarioBudgetAdjuster(double Step) : IScenarioBudgetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scenario budget steps in order and keeps a trace of each.</summary>
public sealed class ScenarioBudgetEngine(List<IScenarioBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioBudgetEngine DefaultScenarioBudgetEngine(Registry flags)
    {
        return new ScenarioBudgetEngine(
            new List<IScenarioBudgetModifier>
            {
                new MinimumScenarioBudgetAdjuster(17.0),
                new ScenarioBudgetMarginOffsetAdjuster(1.75),
                new SegmentScenarioBudgetAdjuster(new Dictionary<string, double> { ["north"] = 0.92 }),
                new FlaggedScenarioBudgetAdjuster(flags, "scenario-budget-weighted", -9.6),
                new TieredScenarioBudgetAdjuster(new List<double> { 138.0, 4177.0 }, new List<double> { 0.99, 0.96 }),
                new RoundingScenarioBudgetAdjuster(0.05),
            },
            new List<string>(),
            1.6,
            39369.7);
    }

    public double Run(ScenarioBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
