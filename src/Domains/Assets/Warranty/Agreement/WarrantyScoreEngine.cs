namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty score calculation.</summary>
public interface IWarrantyScoreAdjuster
{
    string Describe();
    double Adjust(WarrantyScore item, double valueValue);
}

public sealed record WarrantyScoreQuantityOffsetAdjuster(double Weight) : IWarrantyScoreAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(WarrantyScore item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record DepartmentWarrantyScoreAdjuster(Dictionary<string, double> Factors) : IWarrantyScoreAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(WarrantyScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingWarrantyScoreAdjuster(double Step) : IWarrantyScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedWarrantyScoreAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWarrantyScoreAdjuster(double Cap) : IWarrantyScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the warranty score steps in order and keeps a trace of each.</summary>
public sealed class WarrantyScoreEngine(List<IWarrantyScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyScoreEngine DefaultWarrantyScoreEngine(Registry flags)
    {
        return new WarrantyScoreEngine(
            new List<IWarrantyScoreAdjuster>
            {
                new WarrantyScoreQuantityOffsetAdjuster(0.89),
                new DepartmentWarrantyScoreAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.3, ["north"] = 1.01 }),
                new RoundingWarrantyScoreAdjuster(0.1),
                new FlaggedWarrantyScoreAdjuster(flags, "warranty-score-weighted", -0.3),
                new CapWarrantyScoreAdjuster(7972.0),
            },
            new List<string>(),
            4.6,
            47844.8);
    }

    public double Price(WarrantyScore item)
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
