namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate policy calculation.</summary>
public interface IRatePolicyAdjuster
{
    string Describe();
    double Adjust(RatePolicy item, double valueValue);
}

public sealed record MinimumRatePolicyAdjuster(double Minimum) : IRatePolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RatePolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRatePolicyAdjuster(double Percent) : IRatePolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RatePolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RatePolicyTenureMonthsOffsetAdjuster(double Weight) : IRatePolicyAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(RatePolicy item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record TeamRatePolicyAdjuster(Dictionary<string, double> Factors) : IRatePolicyAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RatePolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRatePolicyAdjuster(Registry Flags, string Flag, double Percent) : IRatePolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RatePolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate policy steps in order and keeps a trace of each.</summary>
public sealed class RatePolicyEngine(List<IRatePolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRatePolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RatePolicyEngine DefaultRatePolicyEngine(Registry flags)
    {
        return new RatePolicyEngine(
            new List<IRatePolicyAdjuster>
            {
                new MinimumRatePolicyAdjuster(44.8),
                new PercentRatePolicyAdjuster(15.1),
                new RatePolicyTenureMonthsOffsetAdjuster(1.47),
                new TeamRatePolicyAdjuster(new Dictionary<string, double> { ["west"] = 1.36, ["east"] = 0.79 }),
                new FlaggedRatePolicyAdjuster(flags, "rate-policy-default", -3.2),
            },
            new List<string>(),
            2.3,
            21405.7);
    }

    public double Compute(RatePolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<RatePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
