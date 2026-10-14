namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the strict scenario rule calculation.</summary>
public interface IStrictScenarioRuleAdjuster
{
    string Describe();
    double Adjust(StrictScenarioRule item, double valueValue);
}

public sealed record FlaggedStrictScenarioRuleAdjuster(Registry Flags, string Flag, double Percent) : IStrictScenarioRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictScenarioRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictScenarioRuleAdjuster(double Cap) : IStrictScenarioRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictScenarioRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumStrictScenarioRuleAdjuster(double Minimum) : IStrictScenarioRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictScenarioRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryStrictScenarioRuleAdjuster(Dictionary<string, double> Factors) : IStrictScenarioRuleAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(StrictScenarioRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict scenario rule steps in order and keeps a trace of each.</summary>
public sealed class StrictScenarioRuleEngine(List<IStrictScenarioRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictScenarioRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictScenarioRuleEngine DefaultStrictScenarioRuleEngine(Registry flags)
    {
        return new StrictScenarioRuleEngine(
            new List<IStrictScenarioRuleAdjuster>
            {
                new FlaggedStrictScenarioRuleAdjuster(flags, "strict-scenario-rule-tiered", 0.8),
                new CapStrictScenarioRuleAdjuster(8592.0),
                new MinimumStrictScenarioRuleAdjuster(17.0),
                new CategoryStrictScenarioRuleAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.7, ["north"] = 0.8, ["retail"] = 1.13 }),
            },
            new List<string>(),
            0.4,
            11446.4);
    }

    public double Compute(StrictScenarioRule item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StrictScenarioRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
