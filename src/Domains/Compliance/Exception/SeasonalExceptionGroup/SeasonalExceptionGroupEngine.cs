namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal exception group calculation.</summary>
public interface ISeasonalExceptionGroupStep
{
    string Describe();
    double Adjust(SeasonalExceptionGroup item, double valueValue);
}

public sealed record RoundingSeasonalExceptionGroupAdjuster(double Step) : ISeasonalExceptionGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalExceptionGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelSeasonalExceptionGroupAdjuster(Dictionary<string, double> Factors) : ISeasonalExceptionGroupStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalExceptionGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSeasonalExceptionGroupAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalExceptionGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalExceptionGroup item, double valueValue)
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

public sealed record FlaggedSeasonalExceptionGroupAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalExceptionGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalExceptionGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalExceptionGroupAdjuster(double Cap) : ISeasonalExceptionGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalExceptionGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal exception group steps in order and keeps a trace of each.</summary>
public sealed class SeasonalExceptionGroupEngine(List<ISeasonalExceptionGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalExceptionGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalExceptionGroupEngine DefaultSeasonalExceptionGroupEngine(Registry flags)
    {
        return new SeasonalExceptionGroupEngine(
            new List<ISeasonalExceptionGroupStep>
            {
                new RoundingSeasonalExceptionGroupAdjuster(0.5),
                new LabelSeasonalExceptionGroupAdjuster(new Dictionary<string, double> { ["east"] = 0.75 }),
                new TieredSeasonalExceptionGroupAdjuster(
                    new List<double> { 1760.0, 4369.0, 9606.0 },
                    new List<double> { 0.9, 1.02, 0.84 }),
                new FlaggedSeasonalExceptionGroupAdjuster(flags, "seasonal-exception-group-regional", -5.4),
                new CapSeasonalExceptionGroupAdjuster(17448.0),
            },
            new List<string>(),
            0.9,
            6380.9);
    }

    public double Compute(SeasonalExceptionGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalExceptionGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
