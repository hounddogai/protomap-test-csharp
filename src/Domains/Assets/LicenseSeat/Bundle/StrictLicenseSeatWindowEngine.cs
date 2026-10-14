namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict license seat window calculation.</summary>
public interface IStrictLicenseSeatWindowFactor
{
    string Describe();
    double Adjust(StrictLicenseSeatWindow item, double valueValue);
}

public sealed record StrictLicenseSeatWindowCostOffsetAdjuster(double Weight) : IStrictLicenseSeatWindowFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(StrictLicenseSeatWindow item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record UnitStrictLicenseSeatWindowAdjuster(Dictionary<string, double> Factors) : IStrictLicenseSeatWindowFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(StrictLicenseSeatWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapStrictLicenseSeatWindowAdjuster(double Cap) : IStrictLicenseSeatWindowFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictLicenseSeatWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the strict license seat window steps in order and keeps a trace of each.</summary>
public sealed class StrictLicenseSeatWindowEngine(List<IStrictLicenseSeatWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictLicenseSeatWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictLicenseSeatWindowEngine DefaultStrictLicenseSeatWindowEngine()
    {
        return new StrictLicenseSeatWindowEngine(
            new List<IStrictLicenseSeatWindowFactor>
            {
                new StrictLicenseSeatWindowCostOffsetAdjuster(1.57),
                new UnitStrictLicenseSeatWindowAdjuster(new Dictionary<string, double> { ["east"] = 1.38 }),
                new CapStrictLicenseSeatWindowAdjuster(34816.0),
            },
            new List<string>(),
            0.7,
            44552.3);
    }

    public double Price(StrictLicenseSeatWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictLicenseSeatWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
