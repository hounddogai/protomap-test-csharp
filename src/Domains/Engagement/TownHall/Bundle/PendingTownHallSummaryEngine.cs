namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending town hall summary calculation.</summary>
public interface IPendingTownHallSummaryStep
{
    string Describe();
    double Adjust(PendingTownHallSummary item, double valueValue);
}

public sealed record TieredPendingTownHallSummaryAdjuster(List<double> Bounds, List<double> Rates) : IPendingTownHallSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingTownHallSummary item, double valueValue)
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

public sealed record CapPendingTownHallSummaryAdjuster(double Cap) : IPendingTownHallSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingTownHallSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPendingTownHallSummaryAdjuster(double Step) : IPendingTownHallSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingTownHallSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPendingTownHallSummaryAdjuster(double Percent) : IPendingTownHallSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingTownHallSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending town hall summary steps in order and keeps a trace of each.</summary>
public sealed class PendingTownHallSummaryEngine(List<IPendingTownHallSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingTownHallSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingTownHallSummaryEngine DefaultPendingTownHallSummaryEngine()
    {
        return new PendingTownHallSummaryEngine(
            new List<IPendingTownHallSummaryStep>
            {
                new TieredPendingTownHallSummaryAdjuster(
                    new List<double> { 4467.0, 8808.0, 9968.0 },
                    new List<double> { 1.11, 0.94, 1.18 }),
                new CapPendingTownHallSummaryAdjuster(27692.0),
                new RoundingPendingTownHallSummaryAdjuster(1.0),
                new PercentPendingTownHallSummaryAdjuster(14.5),
            },
            new List<string>(),
            4.0,
            21887.8);
    }

    public double Price(PendingTownHallSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingTownHallSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
