namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling laptop claim calculation.</summary>
public interface IRollingLaptopClaimModifier
{
    string Describe();
    double Adjust(RollingLaptopClaim item, double valueValue);
}

public sealed record PercentRollingLaptopClaimAdjuster(double Percent) : IRollingLaptopClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingLaptopClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingLaptopClaimAdjuster(Registry Flags, string Flag, double Percent) : IRollingLaptopClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingLaptopClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingLaptopClaimAdjuster(double Cap) : IRollingLaptopClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingLaptopClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling laptop claim steps in order and keeps a trace of each.</summary>
public sealed class RollingLaptopClaimEngine(List<IRollingLaptopClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingLaptopClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingLaptopClaimEngine DefaultRollingLaptopClaimEngine(Registry flags)
    {
        return new RollingLaptopClaimEngine(
            new List<IRollingLaptopClaimModifier>
            {
                new PercentRollingLaptopClaimAdjuster(-1.6),
                new FlaggedRollingLaptopClaimAdjuster(flags, "rolling-laptop-claim-strict", -2.9),
                new CapRollingLaptopClaimAdjuster(19662.0),
            },
            new List<string>(),
            1.2,
            36560.2);
    }

    public double Price(RollingLaptopClaim item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingLaptopClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
