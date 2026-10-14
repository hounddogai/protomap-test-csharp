namespace Acme.Workforce.Talent.Nomination.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination segment calculation.</summary>
public interface INominationSegmentStep
{
    string Describe();
    double Adjust(NominationSegment item, double valueValue);
}

public sealed record CategoryNominationSegmentAdjuster(Dictionary<string, double> Factors) : INominationSegmentStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(NominationSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredNominationSegmentAdjuster(List<double> Bounds, List<double> Rates) : INominationSegmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationSegment item, double valueValue)
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

public sealed record RoundingNominationSegmentAdjuster(double Step) : INominationSegmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the nomination segment steps in order and keeps a trace of each.</summary>
public sealed class NominationSegmentEngine(List<INominationSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationSegmentEngine DefaultNominationSegmentEngine()
    {
        return new NominationSegmentEngine(
            new List<INominationSegmentStep>
            {
                new CategoryNominationSegmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.35, ["west"] = 0.74, ["north"] = 1.38 }),
                new TieredNominationSegmentAdjuster(
                    new List<double> { 2908.0, 3448.0 },
                    new List<double> { 1.17, 0.85 }),
                new RoundingNominationSegmentAdjuster(0.25),
            },
            new List<string>(),
            3.5,
            88712.4);
    }

    public double Price(NominationSegment item)
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

    public Dictionary<string, double> EvaluateAll(List<NominationSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
