namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement group calculation.</summary>
public interface IEngagementGroupStep
{
    string Describe();
    double Adjust(EngagementGroup item, double valueValue);
}

public sealed record LabelEngagementGroupAdjuster(Dictionary<string, double> Factors) : IEngagementGroupStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EngagementGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingEngagementGroupAdjuster(double Step) : IEngagementGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapEngagementGroupAdjuster(double Cap) : IEngagementGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredEngagementGroupAdjuster(List<double> Bounds, List<double> Rates) : IEngagementGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementGroup item, double valueValue)
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

public sealed record PercentEngagementGroupAdjuster(double Percent) : IEngagementGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record EngagementGroupGraceDaysOffsetAdjuster(double Weight) : IEngagementGroupStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(EngagementGroup item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the engagement group steps in order and keeps a trace of each.</summary>
public sealed class EngagementGroupEngine(List<IEngagementGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementGroupEngine DefaultEngagementGroupEngine()
    {
        return new EngagementGroupEngine(
            new List<IEngagementGroupStep>
            {
                new LabelEngagementGroupAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.04, ["west"] = 1.22, ["south"] = 1.11 }),
                new RoundingEngagementGroupAdjuster(0.5),
                new CapEngagementGroupAdjuster(20391.0),
                new TieredEngagementGroupAdjuster(
                    new List<double> { 3014.0, 7669.0, 9808.0 },
                    new List<double> { 0.99, 0.95, 0.97 }),
                new PercentEngagementGroupAdjuster(-19.9),
                new EngagementGroupGraceDaysOffsetAdjuster(0.52),
            },
            new List<string>(),
            5.0,
            13345.3);
    }

    public double Evaluate(EngagementGroup item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
