namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped engagement entry calculation.</summary>
public interface ICappedEngagementEntryFactor
{
    string Describe();
    double Adjust(CappedEngagementEntry item, double valueValue);
}

public sealed record PercentCappedEngagementEntryAdjuster(double Percent) : ICappedEngagementEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedEngagementEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedEngagementEntryAdjuster(double Cap) : ICappedEngagementEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedEngagementEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedEngagementEntryScoreOffsetAdjuster(double Weight) : ICappedEngagementEntryFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(CappedEngagementEntry item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the capped engagement entry steps in order and keeps a trace of each.</summary>
public sealed class CappedEngagementEntryEngine(List<ICappedEngagementEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedEngagementEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedEngagementEntryEngine DefaultCappedEngagementEntryEngine()
    {
        return new CappedEngagementEntryEngine(
            new List<ICappedEngagementEntryFactor>
            {
                new PercentCappedEngagementEntryAdjuster(-13.3),
                new CapCappedEngagementEntryAdjuster(4909.0),
                new CappedEngagementEntryScoreOffsetAdjuster(1.74),
            },
            new List<string>(),
            1.0,
            67467.6);
    }

    public double Compute(CappedEngagementEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedEngagementEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
