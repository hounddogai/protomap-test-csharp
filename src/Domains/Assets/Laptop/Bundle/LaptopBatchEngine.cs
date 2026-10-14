namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop batch calculation.</summary>
public interface ILaptopBatchStep
{
    string Describe();
    double Adjust(LaptopBatch item, double valueValue);
}

public sealed record PercentLaptopBatchAdjuster(double Percent) : ILaptopBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLaptopBatchAdjuster(double Cap) : ILaptopBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLaptopBatchAdjuster(double Minimum) : ILaptopBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentLaptopBatchAdjuster(Dictionary<string, double> Factors) : ILaptopBatchStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LaptopBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LaptopBatchGraceDaysOffsetAdjuster(double Weight) : ILaptopBatchStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(LaptopBatch item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the laptop batch steps in order and keeps a trace of each.</summary>
public sealed class LaptopBatchEngine(List<ILaptopBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopBatchEngine DefaultLaptopBatchEngine()
    {
        return new LaptopBatchEngine(
            new List<ILaptopBatchStep>
            {
                new PercentLaptopBatchAdjuster(-23.3),
                new CapLaptopBatchAdjuster(12187.0),
                new MinimumLaptopBatchAdjuster(30.8),
                new SegmentLaptopBatchAdjuster(new Dictionary<string, double> { ["west"] = 0.75 }),
                new LaptopBatchGraceDaysOffsetAdjuster(1.2),
            },
            new List<string>(),
            1.8,
            29305.7);
    }

    public double Run(LaptopBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
