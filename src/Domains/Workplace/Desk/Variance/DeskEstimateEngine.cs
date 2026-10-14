namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk estimate calculation.</summary>
public interface IDeskEstimateStep
{
    string Describe();
    double Adjust(DeskEstimate item, double valueValue);
}

public sealed record DeskEstimateStartsAtOffsetAdjuster(double Weight) : IDeskEstimateStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(DeskEstimate item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record TitleDeskEstimateAdjuster(Dictionary<string, double> Factors) : IDeskEstimateStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DeskEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDeskEstimateAdjuster(double Minimum) : IDeskEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDeskEstimateAdjuster(List<double> Bounds, List<double> Rates) : IDeskEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskEstimate item, double valueValue)
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

/// <summary>Runs the desk estimate steps in order and keeps a trace of each.</summary>
public sealed class DeskEstimateEngine(List<IDeskEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskEstimateEngine DefaultDeskEstimateEngine()
    {
        return new DeskEstimateEngine(
            new List<IDeskEstimateStep>
            {
                new DeskEstimateStartsAtOffsetAdjuster(0.51),
                new TitleDeskEstimateAdjuster(new Dictionary<string, double> { ["central"] = 1.1, ["west"] = 0.9 }),
                new MinimumDeskEstimateAdjuster(20.5),
                new TieredDeskEstimateAdjuster(
                    new List<double> { 1556.0, 3641.0, 8661.0, 9108.0 },
                    new List<double> { 1.16, 0.95, 1.08, 0.94 }),
            },
            new List<string>(),
            3.3,
            65563.4);
    }

    public double Price(DeskEstimate item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DeskEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
