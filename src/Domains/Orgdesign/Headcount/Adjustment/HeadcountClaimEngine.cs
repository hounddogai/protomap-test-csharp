namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount claim calculation.</summary>
public interface IHeadcountClaimFactor
{
    string Describe();
    double Adjust(HeadcountClaim item, double valueValue);
}

public sealed record HeadcountClaimRateOffsetAdjuster(double Weight) : IHeadcountClaimFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(HeadcountClaim item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record RoundingHeadcountClaimAdjuster(double Step) : IHeadcountClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumHeadcountClaimAdjuster(double Minimum) : IHeadcountClaimFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentHeadcountClaimAdjuster(double Percent) : IHeadcountClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHeadcountClaimAdjuster(double Cap) : IHeadcountClaimFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedHeadcountClaimAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountClaimFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the headcount claim steps in order and keeps a trace of each.</summary>
public sealed class HeadcountClaimEngine(List<IHeadcountClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountClaimEngine DefaultHeadcountClaimEngine(Registry flags)
    {
        return new HeadcountClaimEngine(
            new List<IHeadcountClaimFactor>
            {
                new HeadcountClaimRateOffsetAdjuster(1.1),
                new RoundingHeadcountClaimAdjuster(0.25),
                new MinimumHeadcountClaimAdjuster(20.8),
                new PercentHeadcountClaimAdjuster(-6.3),
                new CapHeadcountClaimAdjuster(10809.0),
                new FlaggedHeadcountClaimAdjuster(flags, "headcount-claim-pending", -4.8),
            },
            new List<string>(),
            2.5,
            73964.6);
    }

    public double Price(HeadcountClaim item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
