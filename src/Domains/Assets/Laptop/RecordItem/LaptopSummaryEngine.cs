namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop summary calculation.</summary>
public interface ILaptopSummaryStep
{
    string Describe();
    double Adjust(LaptopSummary item, double valueValue);
}

public sealed record CapLaptopSummaryAdjuster(double Cap) : ILaptopSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LaptopSummaryFloorValueOffsetAdjuster(double Weight) : ILaptopSummaryStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(LaptopSummary item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record PercentLaptopSummaryAdjuster(double Percent) : ILaptopSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelLaptopSummaryAdjuster(Dictionary<string, double> Factors) : ILaptopSummaryStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LaptopSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the laptop summary steps in order and keeps a trace of each.</summary>
public sealed class LaptopSummaryEngine(List<ILaptopSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopSummaryEngine DefaultLaptopSummaryEngine()
    {
        return new LaptopSummaryEngine(
            new List<ILaptopSummaryStep>
            {
                new CapLaptopSummaryAdjuster(4062.0),
                new LaptopSummaryFloorValueOffsetAdjuster(1.31),
                new PercentLaptopSummaryAdjuster(-14.6),
                new ChannelLaptopSummaryAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.34, ["north"] = 1.17 }),
            },
            new List<string>(),
            4.5,
            47273.8);
    }

    public double Run(LaptopSummary item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
