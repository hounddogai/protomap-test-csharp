namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary equity grant override calculation.</summary>
public interface IPrimaryEquityGrantOverrideFactor
{
    string Describe();
    double Adjust(PrimaryEquityGrantOverride item, double valueValue);
}

public sealed record MinimumPrimaryEquityGrantOverrideAdjuster(double Minimum) : IPrimaryEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryEquityGrantOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPrimaryEquityGrantOverrideAdjuster(double Step) : IPrimaryEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryEquityGrantOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryEquityGrantOverrideAdjuster(double Percent) : IPrimaryEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryEquityGrantOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryEquityGrantOverrideAdjuster(double Cap) : IPrimaryEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryEquityGrantOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPrimaryEquityGrantOverrideAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryEquityGrantOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamPrimaryEquityGrantOverrideAdjuster(Dictionary<string, double> Factors) : IPrimaryEquityGrantOverrideFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(PrimaryEquityGrantOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the primary equity grant override steps in order and keeps a trace of each.</summary>
public sealed class PrimaryEquityGrantOverrideEngine(List<IPrimaryEquityGrantOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryEquityGrantOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryEquityGrantOverrideEngine DefaultPrimaryEquityGrantOverrideEngine(Registry flags)
    {
        return new PrimaryEquityGrantOverrideEngine(
            new List<IPrimaryEquityGrantOverrideFactor>
            {
                new MinimumPrimaryEquityGrantOverrideAdjuster(28.7),
                new RoundingPrimaryEquityGrantOverrideAdjuster(0.05),
                new PercentPrimaryEquityGrantOverrideAdjuster(-17.1),
                new CapPrimaryEquityGrantOverrideAdjuster(48513.0),
                new FlaggedPrimaryEquityGrantOverrideAdjuster(flags, "primary-equity-grant-override-fallback", 0.3),
                new TeamPrimaryEquityGrantOverrideAdjuster(new Dictionary<string, double> { ["online"] = 0.78 }),
            },
            new List<string>(),
            1.8,
            26100.2);
    }

    public double Evaluate(PrimaryEquityGrantOverride item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryEquityGrantOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
