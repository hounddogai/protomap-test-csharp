namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the manual garnishment baseline calculation.</summary>
public interface IManualGarnishmentBaselineModifier
{
    string Describe();
    double Adjust(ManualGarnishmentBaseline item, double valueValue);
}

public sealed record CapManualGarnishmentBaselineAdjuster(double Cap) : IManualGarnishmentBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualGarnishmentBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumManualGarnishmentBaselineAdjuster(double Minimum) : IManualGarnishmentBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualGarnishmentBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedManualGarnishmentBaselineAdjuster(Registry Flags, string Flag, double Percent) : IManualGarnishmentBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualGarnishmentBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual garnishment baseline steps in order and keeps a trace of each.</summary>
public sealed class ManualGarnishmentBaselineEngine(List<IManualGarnishmentBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualGarnishmentBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualGarnishmentBaselineEngine DefaultManualGarnishmentBaselineEngine(Registry flags)
    {
        return new ManualGarnishmentBaselineEngine(
            new List<IManualGarnishmentBaselineModifier>
            {
                new CapManualGarnishmentBaselineAdjuster(17902.0),
                new MinimumManualGarnishmentBaselineAdjuster(38.5),
                new FlaggedManualGarnishmentBaselineAdjuster(flags, "manual-garnishment-baseline-capped", 9.4),
            },
            new List<string>(),
            4.3,
            97092.3);
    }

    public double Compute(ManualGarnishmentBaseline item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualGarnishmentBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
