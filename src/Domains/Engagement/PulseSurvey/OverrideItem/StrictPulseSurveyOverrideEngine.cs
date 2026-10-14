namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pulse survey override calculation.</summary>
public interface IStrictPulseSurveyOverrideAdjuster
{
    string Describe();
    double Adjust(StrictPulseSurveyOverride item, double valueValue);
}

public sealed record RoundingStrictPulseSurveyOverrideAdjuster(double Step) : IStrictPulseSurveyOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPulseSurveyOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentStrictPulseSurveyOverrideAdjuster(Dictionary<string, double> Factors) : IStrictPulseSurveyOverrideAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(StrictPulseSurveyOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapStrictPulseSurveyOverrideAdjuster(double Cap) : IStrictPulseSurveyOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPulseSurveyOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedStrictPulseSurveyOverrideAdjuster(Registry Flags, string Flag, double Percent) : IStrictPulseSurveyOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPulseSurveyOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentStrictPulseSurveyOverrideAdjuster(double Percent) : IStrictPulseSurveyOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPulseSurveyOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictPulseSurveyOverrideAdjuster(List<double> Bounds, List<double> Rates) : IStrictPulseSurveyOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPulseSurveyOverride item, double valueValue)
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

/// <summary>Runs the strict pulse survey override steps in order and keeps a trace of each.</summary>
public sealed class StrictPulseSurveyOverrideEngine(List<IStrictPulseSurveyOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPulseSurveyOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPulseSurveyOverrideEngine DefaultStrictPulseSurveyOverrideEngine(Registry flags)
    {
        return new StrictPulseSurveyOverrideEngine(
            new List<IStrictPulseSurveyOverrideAdjuster>
            {
                new RoundingStrictPulseSurveyOverrideAdjuster(0.25),
                new DepartmentStrictPulseSurveyOverrideAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.87, ["retail"] = 1.11, ["north"] = 0.84 }),
                new CapStrictPulseSurveyOverrideAdjuster(8933.0),
                new FlaggedStrictPulseSurveyOverrideAdjuster(flags, "strict-pulse-survey-override-global", 6.1),
                new PercentStrictPulseSurveyOverrideAdjuster(20.4),
                new TieredStrictPulseSurveyOverrideAdjuster(
                    new List<double> { 5598.0, 6613.0, 9935.0 },
                    new List<double> { 1.1, 1.18, 0.88 }),
            },
            new List<string>(),
            3.2,
            89493.1);
    }

    public double Compute(StrictPulseSurveyOverride item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPulseSurveyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
