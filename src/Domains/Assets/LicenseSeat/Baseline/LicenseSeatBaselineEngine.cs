namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat baseline calculation.</summary>
public interface ILicenseSeatBaselineAdjuster
{
    string Describe();
    double Adjust(LicenseSeatBaseline item, double valueValue);
}

public sealed record CapLicenseSeatBaselineAdjuster(double Cap) : ILicenseSeatBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentLicenseSeatBaselineAdjuster(Dictionary<string, double> Factors) : ILicenseSeatBaselineAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LicenseSeatBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LicenseSeatBaselineAmountOffsetAdjuster(double Weight) : ILicenseSeatBaselineAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LicenseSeatBaseline item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentLicenseSeatBaselineAdjuster(double Percent) : ILicenseSeatBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the license seat baseline steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatBaselineEngine(List<ILicenseSeatBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatBaselineEngine DefaultLicenseSeatBaselineEngine()
    {
        return new LicenseSeatBaselineEngine(
            new List<ILicenseSeatBaselineAdjuster>
            {
                new CapLicenseSeatBaselineAdjuster(4438.0),
                new SegmentLicenseSeatBaselineAdjuster(new Dictionary<string, double> { ["south"] = 1.26 }),
                new LicenseSeatBaselineAmountOffsetAdjuster(1.15),
                new PercentLicenseSeatBaselineAdjuster(-3.1),
            },
            new List<string>(),
            2.7,
            34798.5);
    }

    public double Evaluate(LicenseSeatBaseline item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
