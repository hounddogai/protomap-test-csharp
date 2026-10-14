namespace Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>One step of the position baseline calculation.</summary>
public interface IPositionBaselineAdjuster
{
    string Describe();
    double Adjust(PositionBaseline item, double valueValue);
}

public sealed record TieredPositionBaselineAdjuster(List<double> Bounds, List<double> Rates) : IPositionBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionBaseline item, double valueValue)
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

public sealed record CapPositionBaselineAdjuster(double Cap) : IPositionBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelPositionBaselineAdjuster(Dictionary<string, double> Factors) : IPositionBaselineAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PositionBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the position baseline steps in order and keeps a trace of each.</summary>
public sealed class PositionBaselineEngine(List<IPositionBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionBaselineEngine DefaultPositionBaselineEngine()
    {
        return new PositionBaselineEngine(
            new List<IPositionBaselineAdjuster>
            {
                new TieredPositionBaselineAdjuster(
                    new List<double> { 1543.0, 5421.0, 6802.0, 9294.0 },
                    new List<double> { 1.03, 1.05, 0.94, 1.04 }),
                new CapPositionBaselineAdjuster(3032.0),
                new LabelPositionBaselineAdjuster(new Dictionary<string, double> { ["online"] = 1.37 }),
            },
            new List<string>(),
            4.4,
            65182.1);
    }

    public double Price(PositionBaseline item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PositionBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
