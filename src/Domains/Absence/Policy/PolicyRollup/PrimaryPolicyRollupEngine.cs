namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary policy rollup calculation.</summary>
public interface IPrimaryPolicyRollupModifier
{
    string Describe();
    double Adjust(PrimaryPolicyRollup item, double valueValue);
}

public sealed record PercentPrimaryPolicyRollupAdjuster(double Percent) : IPrimaryPolicyRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryPolicyRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPrimaryPolicyRollupAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryPolicyRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryPolicyRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryPolicyRollupLimitOffsetAdjuster(double Weight) : IPrimaryPolicyRollupModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PrimaryPolicyRollup item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record CapPrimaryPolicyRollupAdjuster(double Cap) : IPrimaryPolicyRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryPolicyRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPrimaryPolicyRollupAdjuster(double Step) : IPrimaryPolicyRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryPolicyRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary policy rollup steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPolicyRollupEngine(List<IPrimaryPolicyRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPolicyRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPolicyRollupEngine DefaultPrimaryPolicyRollupEngine(Registry flags)
    {
        return new PrimaryPolicyRollupEngine(
            new List<IPrimaryPolicyRollupModifier>
            {
                new PercentPrimaryPolicyRollupAdjuster(-6.9),
                new FlaggedPrimaryPolicyRollupAdjuster(flags, "primary-policy-rollup-primary", -8.7),
                new PrimaryPolicyRollupLimitOffsetAdjuster(0.87),
                new CapPrimaryPolicyRollupAdjuster(15316.0),
                new RoundingPrimaryPolicyRollupAdjuster(0.25),
            },
            new List<string>(),
            4.7,
            25256.9);
    }

    public double Run(PrimaryPolicyRollup item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPolicyRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
