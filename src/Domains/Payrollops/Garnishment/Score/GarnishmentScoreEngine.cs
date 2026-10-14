namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the garnishment score calculation.</summary>
public interface IGarnishmentScoreFactor
{
    string Describe();
    double Adjust(GarnishmentScore item, double valueValue);
}

public sealed record GarnishmentScoreUpdatedAtOffsetAdjuster(double Weight) : IGarnishmentScoreFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(GarnishmentScore item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record NameGarnishmentScoreAdjuster(Dictionary<string, double> Factors) : IGarnishmentScoreFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GarnishmentScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGarnishmentScoreAdjuster(double Minimum) : IGarnishmentScoreFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedGarnishmentScoreAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentScoreFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the garnishment score steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentScoreEngine(List<IGarnishmentScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentScoreEngine DefaultGarnishmentScoreEngine(Registry flags)
    {
        return new GarnishmentScoreEngine(
            new List<IGarnishmentScoreFactor>
            {
                new GarnishmentScoreUpdatedAtOffsetAdjuster(0.93),
                new NameGarnishmentScoreAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.15, ["online"] = 1.36 }),
                new MinimumGarnishmentScoreAdjuster(32.4),
                new FlaggedGarnishmentScoreAdjuster(flags, "garnishment-score-fallback", -4.5),
            },
            new List<string>(),
            1.2,
            16584.1);
    }

    public double Run(GarnishmentScore item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
