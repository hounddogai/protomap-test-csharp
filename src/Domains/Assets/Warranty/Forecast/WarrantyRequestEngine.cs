namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty request calculation.</summary>
public interface IWarrantyRequestStep
{
    string Describe();
    double Adjust(WarrantyRequest item, double valueValue);
}

public sealed record RoundingWarrantyRequestAdjuster(double Step) : IWarrantyRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleWarrantyRequestAdjuster(Dictionary<string, double> Factors) : IWarrantyRequestStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WarrantyRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentWarrantyRequestAdjuster(double Percent) : IWarrantyRequestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WarrantyRequestFloorValueOffsetAdjuster(double Weight) : IWarrantyRequestStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(WarrantyRequest item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapWarrantyRequestAdjuster(double Cap) : IWarrantyRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumWarrantyRequestAdjuster(double Minimum) : IWarrantyRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the warranty request steps in order and keeps a trace of each.</summary>
public sealed class WarrantyRequestEngine(List<IWarrantyRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyRequestEngine DefaultWarrantyRequestEngine()
    {
        return new WarrantyRequestEngine(
            new List<IWarrantyRequestStep>
            {
                new RoundingWarrantyRequestAdjuster(0.05),
                new TitleWarrantyRequestAdjuster(new Dictionary<string, double> { ["west"] = 0.85 }),
                new PercentWarrantyRequestAdjuster(-12.6),
                new WarrantyRequestFloorValueOffsetAdjuster(0.88),
                new CapWarrantyRequestAdjuster(6521.0),
                new MinimumWarrantyRequestAdjuster(8.8),
            },
            new List<string>(),
            1.4,
            5582.2);
    }

    public double Compute(WarrantyRequest item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
