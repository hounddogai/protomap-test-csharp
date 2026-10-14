namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor entry calculation.</summary>
public interface IVisitorEntryAdjuster
{
    string Describe();
    double Adjust(VisitorEntry item, double valueValue);
}

public sealed record ChannelVisitorEntryAdjuster(Dictionary<string, double> Factors) : IVisitorEntryAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(VisitorEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record VisitorEntryWindowDaysOffsetAdjuster(double Weight) : IVisitorEntryAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(VisitorEntry item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record TieredVisitorEntryAdjuster(List<double> Bounds, List<double> Rates) : IVisitorEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorEntry item, double valueValue)
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

/// <summary>Runs the visitor entry steps in order and keeps a trace of each.</summary>
public sealed class VisitorEntryEngine(List<IVisitorEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorEntryEngine DefaultVisitorEntryEngine()
    {
        return new VisitorEntryEngine(
            new List<IVisitorEntryAdjuster>
            {
                new ChannelVisitorEntryAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.96, ["east"] = 1.25, ["retail"] = 1.25 }),
                new VisitorEntryWindowDaysOffsetAdjuster(1.9),
                new TieredVisitorEntryAdjuster(new List<double> { 2400.0, 2557.0 }, new List<double> { 0.93, 0.93 }),
            },
            new List<string>(),
            2.2,
            90006.2);
    }

    public double Price(VisitorEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<VisitorEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
