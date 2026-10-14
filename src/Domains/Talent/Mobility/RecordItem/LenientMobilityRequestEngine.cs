namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient mobility request calculation.</summary>
public interface ILenientMobilityRequestAdjuster
{
    string Describe();
    double Adjust(LenientMobilityRequest item, double valueValue);
}

public sealed record MinimumLenientMobilityRequestAdjuster(double Minimum) : ILenientMobilityRequestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientMobilityRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientMobilityRequestAdjuster(Registry Flags, string Flag, double Percent) : ILenientMobilityRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientMobilityRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientMobilityRequestAdjuster(List<double> Bounds, List<double> Rates) : ILenientMobilityRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientMobilityRequest item, double valueValue)
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

/// <summary>Runs the lenient mobility request steps in order and keeps a trace of each.</summary>
public sealed class LenientMobilityRequestEngine(List<ILenientMobilityRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientMobilityRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientMobilityRequestEngine DefaultLenientMobilityRequestEngine(Registry flags)
    {
        return new LenientMobilityRequestEngine(
            new List<ILenientMobilityRequestAdjuster>
            {
                new MinimumLenientMobilityRequestAdjuster(19.4),
                new FlaggedLenientMobilityRequestAdjuster(flags, "lenient-mobility-request-adaptive", -5.3),
                new TieredLenientMobilityRequestAdjuster(
                    new List<double> { 2008.0, 4519.0, 5330.0 },
                    new List<double> { 0.96, 0.97, 1.15 }),
            },
            new List<string>(),
            2.5,
            43060.8);
    }

    public double Evaluate(LenientMobilityRequest item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientMobilityRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
