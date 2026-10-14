namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global garnishment target calculation.</summary>
public interface IGlobalGarnishmentTargetFactor
{
    string Describe();
    double Adjust(GlobalGarnishmentTarget item, double valueValue);
}

public sealed record RoundingGlobalGarnishmentTargetAdjuster(double Step) : IGlobalGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalGarnishmentTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedGlobalGarnishmentTargetAdjuster(Registry Flags, string Flag, double Percent) : IGlobalGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalGarnishmentTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGlobalGarnishmentTargetAdjuster(double Cap) : IGlobalGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalGarnishmentTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumGlobalGarnishmentTargetAdjuster(double Minimum) : IGlobalGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalGarnishmentTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentGlobalGarnishmentTargetAdjuster(double Percent) : IGlobalGarnishmentTargetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalGarnishmentTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryGlobalGarnishmentTargetAdjuster(Dictionary<string, double> Factors) : IGlobalGarnishmentTargetFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(GlobalGarnishmentTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global garnishment target steps in order and keeps a trace of each.</summary>
public sealed class GlobalGarnishmentTargetEngine(List<IGlobalGarnishmentTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalGarnishmentTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalGarnishmentTargetEngine DefaultGlobalGarnishmentTargetEngine(Registry flags)
    {
        return new GlobalGarnishmentTargetEngine(
            new List<IGlobalGarnishmentTargetFactor>
            {
                new RoundingGlobalGarnishmentTargetAdjuster(0.1),
                new FlaggedGlobalGarnishmentTargetAdjuster(flags, "global-garnishment-target-pending", -1.0),
                new CapGlobalGarnishmentTargetAdjuster(45483.0),
                new MinimumGlobalGarnishmentTargetAdjuster(5.9),
                new PercentGlobalGarnishmentTargetAdjuster(24.8),
                new CountryGlobalGarnishmentTargetAdjuster(new Dictionary<string, double> { ["online"] = 0.75 }),
            },
            new List<string>(),
            3.6,
            53226.3);
    }

    public double Evaluate(GlobalGarnishmentTarget item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalGarnishmentTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
