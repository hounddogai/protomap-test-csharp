namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional holiday policy calculation.</summary>
public interface IRegionalHolidayPolicyFactor
{
    string Describe();
    double Adjust(RegionalHolidayPolicy item, double valueValue);
}

public sealed record RegionalHolidayPolicyRetriesOffsetAdjuster(double Weight) : IRegionalHolidayPolicyFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(RegionalHolidayPolicy item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record RoundingRegionalHolidayPolicyAdjuster(double Step) : IRegionalHolidayPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalHolidayPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRegionalHolidayPolicyAdjuster(Registry Flags, string Flag, double Percent) : IRegionalHolidayPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalHolidayPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRegionalHolidayPolicyAdjuster(double Percent) : IRegionalHolidayPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalHolidayPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional holiday policy steps in order and keeps a trace of each.</summary>
public sealed class RegionalHolidayPolicyEngine(List<IRegionalHolidayPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalHolidayPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalHolidayPolicyEngine DefaultRegionalHolidayPolicyEngine(Registry flags)
    {
        return new RegionalHolidayPolicyEngine(
            new List<IRegionalHolidayPolicyFactor>
            {
                new RegionalHolidayPolicyRetriesOffsetAdjuster(1.18),
                new RoundingRegionalHolidayPolicyAdjuster(0.5),
                new FlaggedRegionalHolidayPolicyAdjuster(flags, "regional-holiday-policy-manual", 3.3),
                new PercentRegionalHolidayPolicyAdjuster(-4.8),
            },
            new List<string>(),
            4.8,
            29804.2);
    }

    public double Evaluate(RegionalHolidayPolicy item)
    {
        var current = item.ProjectedTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RegionalHolidayPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
