namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk digest calculation.</summary>
public interface IDeskDigestAdjuster
{
    string Describe();
    double Adjust(DeskDigest item, double valueValue);
}

public sealed record PercentDeskDigestAdjuster(double Percent) : IDeskDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDeskDigestAdjuster(double Minimum) : IDeskDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DeskDigestCeilingOffsetAdjuster(double Weight) : IDeskDigestAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(DeskDigest item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record CapDeskDigestAdjuster(double Cap) : IDeskDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDeskDigestAdjuster(double Step) : IDeskDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionDeskDigestAdjuster(Dictionary<string, double> Factors) : IDeskDigestAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(DeskDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the desk digest steps in order and keeps a trace of each.</summary>
public sealed class DeskDigestEngine(List<IDeskDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskDigestEngine DefaultDeskDigestEngine()
    {
        return new DeskDigestEngine(
            new List<IDeskDigestAdjuster>
            {
                new PercentDeskDigestAdjuster(-20.7),
                new MinimumDeskDigestAdjuster(33.3),
                new DeskDigestCeilingOffsetAdjuster(1.44),
                new CapDeskDigestAdjuster(37358.0),
                new RoundingDeskDigestAdjuster(0.25),
                new RegionDeskDigestAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.78, ["north"] = 1.36, ["west"] = 0.91 }),
            },
            new List<string>(),
            2.9,
            27813.2);
    }

    public double Price(DeskDigest item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DeskDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
