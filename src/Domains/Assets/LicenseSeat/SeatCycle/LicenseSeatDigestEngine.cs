namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat digest calculation.</summary>
public interface ILicenseSeatDigestAdjuster
{
    string Describe();
    double Adjust(LicenseSeatDigest item, double valueValue);
}

public sealed record LicenseSeatDigestCostOffsetAdjuster(double Weight) : ILicenseSeatDigestAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(LicenseSeatDigest item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record MinimumLicenseSeatDigestAdjuster(double Minimum) : ILicenseSeatDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameLicenseSeatDigestAdjuster(Dictionary<string, double> Factors) : ILicenseSeatDigestAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LicenseSeatDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLicenseSeatDigestAdjuster(double Percent) : ILicenseSeatDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLicenseSeatDigestAdjuster(double Cap) : ILicenseSeatDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the license seat digest steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatDigestEngine(List<ILicenseSeatDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatDigestEngine DefaultLicenseSeatDigestEngine()
    {
        return new LicenseSeatDigestEngine(
            new List<ILicenseSeatDigestAdjuster>
            {
                new LicenseSeatDigestCostOffsetAdjuster(1.41),
                new MinimumLicenseSeatDigestAdjuster(42.5),
                new NameLicenseSeatDigestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.4, ["east"] = 1.33 }),
                new PercentLicenseSeatDigestAdjuster(-6.0),
                new CapLicenseSeatDigestAdjuster(26559.0),
            },
            new List<string>(),
            2.7,
            61089.1);
    }

    public double Price(LicenseSeatDigest item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
