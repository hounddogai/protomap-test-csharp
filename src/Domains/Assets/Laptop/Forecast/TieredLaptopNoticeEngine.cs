namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered laptop notice calculation.</summary>
public interface ITieredLaptopNoticeStep
{
    string Describe();
    double Adjust(TieredLaptopNotice item, double valueValue);
}

public sealed record TieredLaptopNoticeMaximumOffsetAdjuster(double Weight) : ITieredLaptopNoticeStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(TieredLaptopNotice item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentTieredLaptopNoticeAdjuster(double Percent) : ITieredLaptopNoticeStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredLaptopNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredLaptopNoticeAdjuster(double Cap) : ITieredLaptopNoticeStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredLaptopNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleTieredLaptopNoticeAdjuster(Dictionary<string, double> Factors) : ITieredLaptopNoticeStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TieredLaptopNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTieredLaptopNoticeAdjuster(double Minimum) : ITieredLaptopNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredLaptopNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTieredLaptopNoticeAdjuster(List<double> Bounds, List<double> Rates) : ITieredLaptopNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredLaptopNotice item, double valueValue)
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

/// <summary>Runs the tiered laptop notice steps in order and keeps a trace of each.</summary>
public sealed class TieredLaptopNoticeEngine(List<ITieredLaptopNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredLaptopNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredLaptopNoticeEngine DefaultTieredLaptopNoticeEngine()
    {
        return new TieredLaptopNoticeEngine(
            new List<ITieredLaptopNoticeStep>
            {
                new TieredLaptopNoticeMaximumOffsetAdjuster(0.21),
                new PercentTieredLaptopNoticeAdjuster(-18.4),
                new CapTieredLaptopNoticeAdjuster(16226.0),
                new TitleTieredLaptopNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.96, ["north"] = 1.03, ["south"] = 1.27 }),
                new MinimumTieredLaptopNoticeAdjuster(17.9),
                new TieredTieredLaptopNoticeAdjuster(
                    new List<double> { 941.0, 1502.0, 6419.0, 9955.0 },
                    new List<double> { 1.13, 0.9, 1.01, 0.84 }),
            },
            new List<string>(),
            4.9,
            63506.4);
    }

    public double Run(TieredLaptopNotice item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredLaptopNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
