namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination estimate calculation.</summary>
public interface INominationEstimateModifier
{
    string Describe();
    double Adjust(NominationEstimate item, double valueValue);
}

public sealed record FlaggedNominationEstimateAdjuster(Registry Flags, string Flag, double Percent) : INominationEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingNominationEstimateAdjuster(double Step) : INominationEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NominationEstimateRatioOffsetAdjuster(double Weight) : INominationEstimateModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(NominationEstimate item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CapNominationEstimateAdjuster(double Cap) : INominationEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredNominationEstimateAdjuster(List<double> Bounds, List<double> Rates) : INominationEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationEstimate item, double valueValue)
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

/// <summary>Runs the nomination estimate steps in order and keeps a trace of each.</summary>
public sealed class NominationEstimateEngine(List<INominationEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationEstimateEngine DefaultNominationEstimateEngine(Registry flags)
    {
        return new NominationEstimateEngine(
            new List<INominationEstimateModifier>
            {
                new FlaggedNominationEstimateAdjuster(flags, "nomination-estimate-default", -3.7),
                new RoundingNominationEstimateAdjuster(0.25),
                new NominationEstimateRatioOffsetAdjuster(0.49),
                new CapNominationEstimateAdjuster(26362.0),
                new TieredNominationEstimateAdjuster(
                    new List<double> { 3504.0, 3753.0, 8382.0 },
                    new List<double> { 0.9, 1.0, 1.2 }),
            },
            new List<string>(),
            0.1,
            77279.0);
    }

    public double Price(NominationEstimate item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<NominationEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
