namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending reorg baseline calculation.</summary>
public interface IPendingReorgBaselineFactor
{
    string Describe();
    double Adjust(PendingReorgBaseline item, double valueValue);
}

public sealed record LabelPendingReorgBaselineAdjuster(Dictionary<string, double> Factors) : IPendingReorgBaselineFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PendingReorgBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PendingReorgBaselineEndsAtOffsetAdjuster(double Weight) : IPendingReorgBaselineFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(PendingReorgBaseline item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record RoundingPendingReorgBaselineAdjuster(double Step) : IPendingReorgBaselineFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingReorgBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending reorg baseline steps in order and keeps a trace of each.</summary>
public sealed class PendingReorgBaselineEngine(List<IPendingReorgBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingReorgBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingReorgBaselineEngine DefaultPendingReorgBaselineEngine()
    {
        return new PendingReorgBaselineEngine(
            new List<IPendingReorgBaselineFactor>
            {
                new LabelPendingReorgBaselineAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.03, ["west"] = 1.37, ["north"] = 0.83 }),
                new PendingReorgBaselineEndsAtOffsetAdjuster(1.11),
                new RoundingPendingReorgBaselineAdjuster(0.5),
            },
            new List<string>(),
            0.3,
            13216.0);
    }

    public double Run(PendingReorgBaseline item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingReorgBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
