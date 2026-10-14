namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy scenario override calculation.</summary>
public interface ILegacyScenarioOverrideStep
{
    string Describe();
    double Adjust(LegacyScenarioOverride item, double valueValue);
}

public sealed record RoundingLegacyScenarioOverrideAdjuster(double Step) : ILegacyScenarioOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyScenarioOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyScenarioOverrideAdjuster(double Percent) : ILegacyScenarioOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyScenarioOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyScenarioOverrideAdjuster(List<double> Bounds, List<double> Rates) : ILegacyScenarioOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyScenarioOverride item, double valueValue)
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

public sealed record MinimumLegacyScenarioOverrideAdjuster(double Minimum) : ILegacyScenarioOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyScenarioOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LegacyScenarioOverrideFteOffsetAdjuster(double Weight) : ILegacyScenarioOverrideStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(LegacyScenarioOverride item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record FlaggedLegacyScenarioOverrideAdjuster(Registry Flags, string Flag, double Percent) : ILegacyScenarioOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyScenarioOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy scenario override steps in order and keeps a trace of each.</summary>
public sealed class LegacyScenarioOverrideEngine(List<ILegacyScenarioOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyScenarioOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyScenarioOverrideEngine DefaultLegacyScenarioOverrideEngine(Registry flags)
    {
        return new LegacyScenarioOverrideEngine(
            new List<ILegacyScenarioOverrideStep>
            {
                new RoundingLegacyScenarioOverrideAdjuster(0.1),
                new PercentLegacyScenarioOverrideAdjuster(21.8),
                new TieredLegacyScenarioOverrideAdjuster(
                    new List<double> { 1992.0, 6301.0, 9048.0, 9538.0 },
                    new List<double> { 0.84, 1.12, 0.94, 1.07 }),
                new MinimumLegacyScenarioOverrideAdjuster(16.0),
                new LegacyScenarioOverrideFteOffsetAdjuster(1.47),
                new FlaggedLegacyScenarioOverrideAdjuster(flags, "legacy-scenario-override-adaptive", -2.0),
            },
            new List<string>(),
            1.5,
            2852.6);
    }

    public double Evaluate(LegacyScenarioOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyScenarioOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
