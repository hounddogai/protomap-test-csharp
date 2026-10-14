namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position group calculation.</summary>
public interface IPositionGroupAdjuster
{
    string Describe();
    double Adjust(PositionGroup item, double valueValue);
}

public sealed record FlaggedPositionGroupAdjuster(Registry Flags, string Flag, double Percent) : IPositionGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPositionGroupAdjuster(List<double> Bounds, List<double> Rates) : IPositionGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionGroup item, double valueValue)
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

public sealed record RoundingPositionGroupAdjuster(double Step) : IPositionGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelPositionGroupAdjuster(Dictionary<string, double> Factors) : IPositionGroupAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PositionGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the position group steps in order and keeps a trace of each.</summary>
public sealed class PositionGroupEngine(List<IPositionGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionGroupEngine DefaultPositionGroupEngine(Registry flags)
    {
        return new PositionGroupEngine(
            new List<IPositionGroupAdjuster>
            {
                new FlaggedPositionGroupAdjuster(flags, "position-group-capped", -7.5),
                new TieredPositionGroupAdjuster(
                    new List<double> { 284.0, 5991.0, 8229.0, 9345.0 },
                    new List<double> { 0.95, 1.18, 0.91, 0.87 }),
                new RoundingPositionGroupAdjuster(0.5),
                new LabelPositionGroupAdjuster(new Dictionary<string, double> { ["west"] = 1.36 }),
            },
            new List<string>(),
            3.9,
            94846.3);
    }

    public double Price(PositionGroup item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
