namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback cost model score calculation.</summary>
public interface IFallbackCostModelScoreAdjuster
{
    string Describe();
    double Adjust(FallbackCostModelScore item, double valueValue);
}

public sealed record RoundingFallbackCostModelScoreAdjuster(double Step) : IFallbackCostModelScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackCostModelScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFallbackCostModelScoreAdjuster(List<double> Bounds, List<double> Rates) : IFallbackCostModelScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackCostModelScore item, double valueValue)
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

public sealed record TitleFallbackCostModelScoreAdjuster(Dictionary<string, double> Factors) : IFallbackCostModelScoreAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FallbackCostModelScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentFallbackCostModelScoreAdjuster(double Percent) : IFallbackCostModelScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackCostModelScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackCostModelScoreRatingOffsetAdjuster(double Weight) : IFallbackCostModelScoreAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(FallbackCostModelScore item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record MinimumFallbackCostModelScoreAdjuster(double Minimum) : IFallbackCostModelScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackCostModelScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback cost model score steps in order and keeps a trace of each.</summary>
public sealed class FallbackCostModelScoreEngine(List<IFallbackCostModelScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackCostModelScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackCostModelScoreEngine DefaultFallbackCostModelScoreEngine()
    {
        return new FallbackCostModelScoreEngine(
            new List<IFallbackCostModelScoreAdjuster>
            {
                new RoundingFallbackCostModelScoreAdjuster(0.05),
                new TieredFallbackCostModelScoreAdjuster(
                    new List<double> { 3923.0, 4716.0 },
                    new List<double> { 1.0, 1.1 }),
                new TitleFallbackCostModelScoreAdjuster(new Dictionary<string, double> { ["east"] = 1.12 }),
                new PercentFallbackCostModelScoreAdjuster(-13.0),
                new FallbackCostModelScoreRatingOffsetAdjuster(1.61),
                new MinimumFallbackCostModelScoreAdjuster(24.0),
            },
            new List<string>(),
            4.5,
            90399.2);
    }

    public double Compute(FallbackCostModelScore item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackCostModelScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
