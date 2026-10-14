namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination variance calculation.</summary>
public interface INominationVarianceAdjuster
{
    string Describe();
    double Adjust(NominationVariance item, double valueValue);
}

public sealed record PercentNominationVarianceAdjuster(double Percent) : INominationVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationVarianceAdjuster(List<double> Bounds, List<double> Rates) : INominationVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationVariance item, double valueValue)
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

public sealed record RoundingNominationVarianceAdjuster(double Step) : INominationVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelNominationVarianceAdjuster(Dictionary<string, double> Factors) : INominationVarianceAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(NominationVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumNominationVarianceAdjuster(double Minimum) : INominationVarianceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the nomination variance steps in order and keeps a trace of each.</summary>
public sealed class NominationVarianceEngine(List<INominationVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationVarianceEngine DefaultNominationVarianceEngine()
    {
        return new NominationVarianceEngine(
            new List<INominationVarianceAdjuster>
            {
                new PercentNominationVarianceAdjuster(-23.9),
                new TieredNominationVarianceAdjuster(
                    new List<double> { 2134.0, 4432.0 },
                    new List<double> { 1.07, 1.11 }),
                new RoundingNominationVarianceAdjuster(1.0),
                new LabelNominationVarianceAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.06, ["retail"] = 1.07 }),
                new MinimumNominationVarianceAdjuster(34.7),
            },
            new List<string>(),
            2.1,
            22127.6);
    }

    public double Price(NominationVariance item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<NominationVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
