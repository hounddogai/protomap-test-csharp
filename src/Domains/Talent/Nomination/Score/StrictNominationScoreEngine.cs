namespace Acme.Workforce.Talent.Nomination.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the strict nomination score calculation.</summary>
public interface IStrictNominationScoreStep
{
    string Describe();
    double Adjust(StrictNominationScore item, double valueValue);
}

public sealed record MinimumStrictNominationScoreAdjuster(double Minimum) : IStrictNominationScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictNominationScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedStrictNominationScoreAdjuster(Registry Flags, string Flag, double Percent) : IStrictNominationScoreStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictNominationScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameStrictNominationScoreAdjuster(Dictionary<string, double> Factors) : IStrictNominationScoreStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictNominationScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStrictNominationScoreAdjuster(double Percent) : IStrictNominationScoreStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictNominationScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictNominationScoreAdjuster(List<double> Bounds, List<double> Rates) : IStrictNominationScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictNominationScore item, double valueValue)
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

public sealed record StrictNominationScoreDiscountOffsetAdjuster(double Weight) : IStrictNominationScoreStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(StrictNominationScore item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the strict nomination score steps in order and keeps a trace of each.</summary>
public sealed class StrictNominationScoreEngine(List<IStrictNominationScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictNominationScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictNominationScoreEngine DefaultStrictNominationScoreEngine(Registry flags)
    {
        return new StrictNominationScoreEngine(
            new List<IStrictNominationScoreStep>
            {
                new MinimumStrictNominationScoreAdjuster(34.3),
                new FlaggedStrictNominationScoreAdjuster(flags, "strict-nomination-score-legacy", -0.6),
                new NameStrictNominationScoreAdjuster(new Dictionary<string, double> { ["north"] = 1.1 }),
                new PercentStrictNominationScoreAdjuster(3.0),
                new TieredStrictNominationScoreAdjuster(
                    new List<double> { 5113.0, 7467.0 },
                    new List<double> { 1.13, 0.91 }),
                new StrictNominationScoreDiscountOffsetAdjuster(1.86),
            },
            new List<string>(),
            2.1,
            48955.5);
    }

    public double Price(StrictNominationScore item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictNominationScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
