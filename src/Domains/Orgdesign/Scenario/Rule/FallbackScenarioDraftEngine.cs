namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback scenario draft calculation.</summary>
public interface IFallbackScenarioDraftFactor
{
    string Describe();
    double Adjust(FallbackScenarioDraft item, double valueValue);
}

public sealed record RoundingFallbackScenarioDraftAdjuster(double Step) : IFallbackScenarioDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackScenarioDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedFallbackScenarioDraftAdjuster(Registry Flags, string Flag, double Percent) : IFallbackScenarioDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackScenarioDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackScenarioDraftAdjuster(double Percent) : IFallbackScenarioDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackScenarioDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelFallbackScenarioDraftAdjuster(Dictionary<string, double> Factors) : IFallbackScenarioDraftFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FallbackScenarioDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapFallbackScenarioDraftAdjuster(double Cap) : IFallbackScenarioDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackScenarioDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback scenario draft steps in order and keeps a trace of each.</summary>
public sealed class FallbackScenarioDraftEngine(List<IFallbackScenarioDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackScenarioDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackScenarioDraftEngine DefaultFallbackScenarioDraftEngine(Registry flags)
    {
        return new FallbackScenarioDraftEngine(
            new List<IFallbackScenarioDraftFactor>
            {
                new RoundingFallbackScenarioDraftAdjuster(5.0),
                new FlaggedFallbackScenarioDraftAdjuster(flags, "fallback-scenario-draft-weighted", 5.1),
                new PercentFallbackScenarioDraftAdjuster(-8.3),
                new LabelFallbackScenarioDraftAdjuster(new Dictionary<string, double> { ["west"] = 1.03 }),
                new CapFallbackScenarioDraftAdjuster(2618.0),
            },
            new List<string>(),
            1.8,
            36853.4);
    }

    public double Compute(FallbackScenarioDraft item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackScenarioDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
