namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination group calculation.</summary>
public interface INominationGroupModifier
{
    string Describe();
    double Adjust(NominationGroup item, double valueValue);
}

public sealed record RoundingNominationGroupAdjuster(double Step) : INominationGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumNominationGroupAdjuster(double Minimum) : INominationGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentNominationGroupAdjuster(double Percent) : INominationGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationGroupAdjuster(List<double> Bounds, List<double> Rates) : INominationGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationGroup item, double valueValue)
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

/// <summary>Runs the nomination group steps in order and keeps a trace of each.</summary>
public sealed class NominationGroupEngine(List<INominationGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationGroupEngine DefaultNominationGroupEngine()
    {
        return new NominationGroupEngine(
            new List<INominationGroupModifier>
            {
                new RoundingNominationGroupAdjuster(5.0),
                new MinimumNominationGroupAdjuster(19.4),
                new PercentNominationGroupAdjuster(11.7),
                new TieredNominationGroupAdjuster(
                    new List<double> { 63.0, 1471.0, 7617.0, 9674.0 },
                    new List<double> { 1.13, 0.98, 1.14, 1.01 }),
            },
            new List<string>(),
            2.2,
            70607.2);
    }

    public double Compute(NominationGroup item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<NominationGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
