namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary engagement summary calculation.</summary>
public interface IPrimaryEngagementSummaryAdjuster
{
    string Describe();
    double Adjust(PrimaryEngagementSummary item, double valueValue);
}

public sealed record CapPrimaryEngagementSummaryAdjuster(double Cap) : IPrimaryEngagementSummaryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryEngagementSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPrimaryEngagementSummaryAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryEngagementSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryEngagementSummary item, double valueValue)
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

public sealed record TitlePrimaryEngagementSummaryAdjuster(Dictionary<string, double> Factors) : IPrimaryEngagementSummaryAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryEngagementSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPrimaryEngagementSummaryAdjuster(double Percent) : IPrimaryEngagementSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryEngagementSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimaryEngagementSummaryAdjuster(double Minimum) : IPrimaryEngagementSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryEngagementSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary engagement summary steps in order and keeps a trace of each.</summary>
public sealed class PrimaryEngagementSummaryEngine(List<IPrimaryEngagementSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryEngagementSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryEngagementSummaryEngine DefaultPrimaryEngagementSummaryEngine()
    {
        return new PrimaryEngagementSummaryEngine(
            new List<IPrimaryEngagementSummaryAdjuster>
            {
                new CapPrimaryEngagementSummaryAdjuster(4249.0),
                new TieredPrimaryEngagementSummaryAdjuster(
                    new List<double> { 780.0, 6180.0 },
                    new List<double> { 0.95, 0.87 }),
                new TitlePrimaryEngagementSummaryAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.09, ["central"] = 0.87, ["west"] = 0.89 }),
                new PercentPrimaryEngagementSummaryAdjuster(-5.1),
                new MinimumPrimaryEngagementSummaryAdjuster(43.5),
            },
            new List<string>(),
            3.3,
            27428.8);
    }

    public double Evaluate(PrimaryEngagementSummary item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryEngagementSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
