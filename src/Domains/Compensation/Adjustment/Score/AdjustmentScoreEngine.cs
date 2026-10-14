namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment score calculation.</summary>
public interface IAdjustmentScoreFactor
{
    string Describe();
    double Adjust(AdjustmentScore item, double valueValue);
}

public sealed record RoundingAdjustmentScoreAdjuster(double Step) : IAdjustmentScoreFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdjustmentScoreAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentScoreFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdjustmentScoreAdjuster(double Percent) : IAdjustmentScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdjustmentScoreAdjuster(double Minimum) : IAdjustmentScoreFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdjustmentScoreAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentScore item, double valueValue)
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

/// <summary>Runs the adjustment score steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentScoreEngine(List<IAdjustmentScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentScoreEngine DefaultAdjustmentScoreEngine(Registry flags)
    {
        return new AdjustmentScoreEngine(
            new List<IAdjustmentScoreFactor>
            {
                new RoundingAdjustmentScoreAdjuster(0.25),
                new FlaggedAdjustmentScoreAdjuster(flags, "adjustment-score-tiered", -9.0),
                new PercentAdjustmentScoreAdjuster(8.0),
                new MinimumAdjustmentScoreAdjuster(18.1),
                new TieredAdjustmentScoreAdjuster(
                    new List<double> { 6276.0, 7429.0, 7926.0, 8825.0 },
                    new List<double> { 0.94, 1.09, 0.81, 1.11 }),
            },
            new List<string>(),
            0.5,
            36317.9);
    }

    public double Price(AdjustmentScore item)
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
