namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient scenario cycle calculation.</summary>
public interface ILenientScenarioCycleStep
{
    string Describe();
    double Adjust(LenientScenarioCycle item, double valueValue);
}

public sealed record MinimumLenientScenarioCycleAdjuster(double Minimum) : ILenientScenarioCycleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientScenarioCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LenientScenarioCycleEndsAtOffsetAdjuster(double Weight) : ILenientScenarioCycleStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(LenientScenarioCycle item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record PercentLenientScenarioCycleAdjuster(double Percent) : ILenientScenarioCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientScenarioCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLenientScenarioCycleAdjuster(Registry Flags, string Flag, double Percent) : ILenientScenarioCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientScenarioCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientScenarioCycleAdjuster(double Cap) : ILenientScenarioCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientScenarioCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientScenarioCycleAdjuster(double Step) : ILenientScenarioCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientScenarioCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient scenario cycle steps in order and keeps a trace of each.</summary>
public sealed class LenientScenarioCycleEngine(List<ILenientScenarioCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientScenarioCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientScenarioCycleEngine DefaultLenientScenarioCycleEngine(Registry flags)
    {
        return new LenientScenarioCycleEngine(
            new List<ILenientScenarioCycleStep>
            {
                new MinimumLenientScenarioCycleAdjuster(7.9),
                new LenientScenarioCycleEndsAtOffsetAdjuster(1.83),
                new PercentLenientScenarioCycleAdjuster(-17.8),
                new FlaggedLenientScenarioCycleAdjuster(flags, "lenient-scenario-cycle-regional", -0.7),
                new CapLenientScenarioCycleAdjuster(10128.0),
                new RoundingLenientScenarioCycleAdjuster(0.5),
            },
            new List<string>(),
            0.8,
            58139.2);
    }

    public double Evaluate(LenientScenarioCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientScenarioCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
