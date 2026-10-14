namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the seasonal reorg exception calculation.</summary>
public interface ISeasonalReorgExceptionStep
{
    string Describe();
    double Adjust(SeasonalReorgException item, double valueValue);
}

public sealed record FlaggedSeasonalReorgExceptionAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalReorgExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalReorgException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalReorgExceptionAdjuster(double Cap) : ISeasonalReorgExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalReorgException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalReorgExceptionAdjuster(double Minimum) : ISeasonalReorgExceptionStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalReorgException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSeasonalReorgExceptionAdjuster(double Percent) : ISeasonalReorgExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalReorgException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal reorg exception steps in order and keeps a trace of each.</summary>
public sealed class SeasonalReorgExceptionEngine(List<ISeasonalReorgExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalReorgExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalReorgExceptionEngine DefaultSeasonalReorgExceptionEngine(Registry flags)
    {
        return new SeasonalReorgExceptionEngine(
            new List<ISeasonalReorgExceptionStep>
            {
                new FlaggedSeasonalReorgExceptionAdjuster(flags, "seasonal-reorg-exception-weighted", 1.9),
                new CapSeasonalReorgExceptionAdjuster(39088.0),
                new MinimumSeasonalReorgExceptionAdjuster(42.0),
                new PercentSeasonalReorgExceptionAdjuster(-0.7),
            },
            new List<string>(),
            4.2,
            75067.1);
    }

    public double Compute(SeasonalReorgException item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalReorgException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
