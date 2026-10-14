namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped occupancy policy calculation.</summary>
public interface ICappedOccupancyPolicyStep
{
    string Describe();
    double Adjust(CappedOccupancyPolicy item, double valueValue);
}

public sealed record PercentCappedOccupancyPolicyAdjuster(double Percent) : ICappedOccupancyPolicyStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedOccupancyPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedOccupancyPolicyAttemptsOffsetAdjuster(double Weight) : ICappedOccupancyPolicyStep
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(CappedOccupancyPolicy item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record ChannelCappedOccupancyPolicyAdjuster(Dictionary<string, double> Factors) : ICappedOccupancyPolicyStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CappedOccupancyPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCappedOccupancyPolicyAdjuster(Registry Flags, string Flag, double Percent) : ICappedOccupancyPolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedOccupancyPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedOccupancyPolicyAdjuster(double Step) : ICappedOccupancyPolicyStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedOccupancyPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapCappedOccupancyPolicyAdjuster(double Cap) : ICappedOccupancyPolicyStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedOccupancyPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped occupancy policy steps in order and keeps a trace of each.</summary>
public sealed class CappedOccupancyPolicyEngine(List<ICappedOccupancyPolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedOccupancyPolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedOccupancyPolicyEngine DefaultCappedOccupancyPolicyEngine(Registry flags)
    {
        return new CappedOccupancyPolicyEngine(
            new List<ICappedOccupancyPolicyStep>
            {
                new PercentCappedOccupancyPolicyAdjuster(-4.0),
                new CappedOccupancyPolicyAttemptsOffsetAdjuster(0.33),
                new ChannelCappedOccupancyPolicyAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.38, ["retail"] = 1.28, ["online"] = 1.09 }),
                new FlaggedCappedOccupancyPolicyAdjuster(flags, "capped-occupancy-policy-capped", -2.3),
                new RoundingCappedOccupancyPolicyAdjuster(0.5),
                new CapCappedOccupancyPolicyAdjuster(11930.0),
            },
            new List<string>(),
            3.3,
            49896.9);
    }

    public double Price(CappedOccupancyPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedOccupancyPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
