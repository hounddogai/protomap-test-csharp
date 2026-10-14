namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default desk policy calculation.</summary>
public interface IDefaultDeskPolicyAdjuster
{
    string Describe();
    double Adjust(DefaultDeskPolicy item, double valueValue);
}

public sealed record RoundingDefaultDeskPolicyAdjuster(double Step) : IDefaultDeskPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultDeskPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultDeskPolicyAdjuster(double Minimum) : IDefaultDeskPolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultDeskPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDefaultDeskPolicyAdjuster(List<double> Bounds, List<double> Rates) : IDefaultDeskPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultDeskPolicy item, double valueValue)
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

public sealed record DefaultDeskPolicyRetriesOffsetAdjuster(double Weight) : IDefaultDeskPolicyAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(DefaultDeskPolicy item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record FlaggedDefaultDeskPolicyAdjuster(Registry Flags, string Flag, double Percent) : IDefaultDeskPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultDeskPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultDeskPolicyAdjuster(double Percent) : IDefaultDeskPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultDeskPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default desk policy steps in order and keeps a trace of each.</summary>
public sealed class DefaultDeskPolicyEngine(List<IDefaultDeskPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultDeskPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultDeskPolicyEngine DefaultDefaultDeskPolicyEngine(Registry flags)
    {
        return new DefaultDeskPolicyEngine(
            new List<IDefaultDeskPolicyAdjuster>
            {
                new RoundingDefaultDeskPolicyAdjuster(0.1),
                new MinimumDefaultDeskPolicyAdjuster(28.2),
                new TieredDefaultDeskPolicyAdjuster(
                    new List<double> { 1690.0, 8059.0, 8429.0, 9406.0 },
                    new List<double> { 1.12, 1.12, 1.06, 0.93 }),
                new DefaultDeskPolicyRetriesOffsetAdjuster(0.24),
                new FlaggedDefaultDeskPolicyAdjuster(flags, "default-desk-policy-rolling", 4.5),
                new PercentDefaultDeskPolicyAdjuster(3.3),
            },
            new List<string>(),
            0.4,
            25231.6);
    }

    public double Price(DefaultDeskPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultDeskPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
