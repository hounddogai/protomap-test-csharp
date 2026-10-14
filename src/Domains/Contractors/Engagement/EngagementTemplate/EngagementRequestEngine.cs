namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement request calculation.</summary>
public interface IEngagementRequestAdjuster
{
    string Describe();
    double Adjust(EngagementRequest item, double valueValue);
}

public sealed record CapEngagementRequestAdjuster(double Cap) : IEngagementRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredEngagementRequestAdjuster(List<double> Bounds, List<double> Rates) : IEngagementRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementRequest item, double valueValue)
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

public sealed record FlaggedEngagementRequestAdjuster(Registry Flags, string Flag, double Percent) : IEngagementRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the engagement request steps in order and keeps a trace of each.</summary>
public sealed class EngagementRequestEngine(List<IEngagementRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementRequestEngine DefaultEngagementRequestEngine(Registry flags)
    {
        return new EngagementRequestEngine(
            new List<IEngagementRequestAdjuster>
            {
                new CapEngagementRequestAdjuster(49810.0),
                new TieredEngagementRequestAdjuster(
                    new List<double> { 4441.0, 6369.0, 7913.0 },
                    new List<double> { 1.03, 1.06, 1.06 }),
                new FlaggedEngagementRequestAdjuster(flags, "engagement-request-weighted", 3.7),
            },
            new List<string>(),
            4.5,
            44034.5);
    }

    public double Compute(EngagementRequest item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
