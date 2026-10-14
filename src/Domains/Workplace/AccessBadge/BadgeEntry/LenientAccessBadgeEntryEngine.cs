namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient access badge entry calculation.</summary>
public interface ILenientAccessBadgeEntryAdjuster
{
    string Describe();
    double Adjust(LenientAccessBadgeEntry item, double valueValue);
}

public sealed record TieredLenientAccessBadgeEntryAdjuster(List<double> Bounds, List<double> Rates) : ILenientAccessBadgeEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientAccessBadgeEntry item, double valueValue)
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

public sealed record RoundingLenientAccessBadgeEntryAdjuster(double Step) : ILenientAccessBadgeEntryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientAccessBadgeEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLenientAccessBadgeEntryAdjuster(double Minimum) : ILenientAccessBadgeEntryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientAccessBadgeEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLenientAccessBadgeEntryAdjuster(double Percent) : ILenientAccessBadgeEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientAccessBadgeEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientAccessBadgeEntryQuantityOffsetAdjuster(double Weight) : ILenientAccessBadgeEntryAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(LenientAccessBadgeEntry item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the lenient access badge entry steps in order and keeps a trace of each.</summary>
public sealed class LenientAccessBadgeEntryEngine(List<ILenientAccessBadgeEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientAccessBadgeEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientAccessBadgeEntryEngine DefaultLenientAccessBadgeEntryEngine()
    {
        return new LenientAccessBadgeEntryEngine(
            new List<ILenientAccessBadgeEntryAdjuster>
            {
                new TieredLenientAccessBadgeEntryAdjuster(
                    new List<double> { 484.0, 3957.0, 4441.0 },
                    new List<double> { 0.88, 1.07, 0.95 }),
                new RoundingLenientAccessBadgeEntryAdjuster(1.0),
                new MinimumLenientAccessBadgeEntryAdjuster(16.0),
                new PercentLenientAccessBadgeEntryAdjuster(10.2),
                new LenientAccessBadgeEntryQuantityOffsetAdjuster(1.63),
            },
            new List<string>(),
            2.2,
            3820.5);
    }

    public double Run(LenientAccessBadgeEntry item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientAccessBadgeEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
