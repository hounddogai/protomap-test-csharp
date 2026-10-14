namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted nomination variance calculation.</summary>
public interface IWeightedNominationVarianceModifier
{
    string Describe();
    double Adjust(WeightedNominationVariance item, double valueValue);
}

public sealed record PercentWeightedNominationVarianceAdjuster(double Percent) : IWeightedNominationVarianceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedNominationVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamWeightedNominationVarianceAdjuster(Dictionary<string, double> Factors) : IWeightedNominationVarianceModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(WeightedNominationVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedNominationVarianceAdjuster(double Minimum) : IWeightedNominationVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedNominationVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record WeightedNominationVarianceAmountOffsetAdjuster(double Weight) : IWeightedNominationVarianceModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(WeightedNominationVariance item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the weighted nomination variance steps in order and keeps a trace of each.</summary>
public sealed class WeightedNominationVarianceEngine(List<IWeightedNominationVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedNominationVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedNominationVarianceEngine DefaultWeightedNominationVarianceEngine()
    {
        return new WeightedNominationVarianceEngine(
            new List<IWeightedNominationVarianceModifier>
            {
                new PercentWeightedNominationVarianceAdjuster(16.3),
                new TeamWeightedNominationVarianceAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.35, ["north"] = 1.15 }),
                new MinimumWeightedNominationVarianceAdjuster(20.8),
                new WeightedNominationVarianceAmountOffsetAdjuster(1.91),
            },
            new List<string>(),
            0.9,
            35638.0);
    }

    public double Compute(WeightedNominationVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedNominationVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
