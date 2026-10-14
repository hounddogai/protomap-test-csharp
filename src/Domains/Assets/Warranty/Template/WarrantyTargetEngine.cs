namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty target calculation.</summary>
public interface IWarrantyTargetStep
{
    string Describe();
    double Adjust(WarrantyTarget item, double valueValue);
}

public sealed record WarrantyTargetQuantityOffsetAdjuster(double Weight) : IWarrantyTargetStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(WarrantyTarget item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record CapWarrantyTargetAdjuster(double Cap) : IWarrantyTargetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumWarrantyTargetAdjuster(double Minimum) : IWarrantyTargetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWarrantyTargetAdjuster(double Percent) : IWarrantyTargetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryWarrantyTargetAdjuster(Dictionary<string, double> Factors) : IWarrantyTargetStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(WarrantyTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredWarrantyTargetAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyTarget item, double valueValue)
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

/// <summary>Runs the warranty target steps in order and keeps a trace of each.</summary>
public sealed class WarrantyTargetEngine(List<IWarrantyTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyTargetEngine DefaultWarrantyTargetEngine()
    {
        return new WarrantyTargetEngine(
            new List<IWarrantyTargetStep>
            {
                new WarrantyTargetQuantityOffsetAdjuster(1.44),
                new CapWarrantyTargetAdjuster(15547.0),
                new MinimumWarrantyTargetAdjuster(43.4),
                new PercentWarrantyTargetAdjuster(-17.5),
                new CategoryWarrantyTargetAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.27, ["south"] = 1.11 }),
                new TieredWarrantyTargetAdjuster(
                    new List<double> { 234.0, 2512.0, 7975.0 },
                    new List<double> { 0.85, 1.16, 1.07 }),
            },
            new List<string>(),
            3.8,
            24078.7);
    }

    public double Run(WarrantyTarget item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
