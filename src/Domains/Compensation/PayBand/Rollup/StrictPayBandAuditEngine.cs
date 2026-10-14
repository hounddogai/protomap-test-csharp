namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pay band audit calculation.</summary>
public interface IStrictPayBandAuditFactor
{
    string Describe();
    double Adjust(StrictPayBandAudit item, double valueValue);
}

public sealed record MinimumStrictPayBandAuditAdjuster(double Minimum) : IStrictPayBandAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPayBandAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedStrictPayBandAuditAdjuster(Registry Flags, string Flag, double Percent) : IStrictPayBandAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPayBandAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentStrictPayBandAuditAdjuster(double Percent) : IStrictPayBandAuditFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPayBandAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictPayBandAuditAdjuster(List<double> Bounds, List<double> Rates) : IStrictPayBandAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPayBandAudit item, double valueValue)
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

public sealed record RoundingStrictPayBandAuditAdjuster(double Step) : IStrictPayBandAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPayBandAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict pay band audit steps in order and keeps a trace of each.</summary>
public sealed class StrictPayBandAuditEngine(List<IStrictPayBandAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPayBandAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPayBandAuditEngine DefaultStrictPayBandAuditEngine(Registry flags)
    {
        return new StrictPayBandAuditEngine(
            new List<IStrictPayBandAuditFactor>
            {
                new MinimumStrictPayBandAuditAdjuster(19.1),
                new FlaggedStrictPayBandAuditAdjuster(flags, "strict-pay-band-audit-lenient", 2.2),
                new PercentStrictPayBandAuditAdjuster(-17.7),
                new TieredStrictPayBandAuditAdjuster(
                    new List<double> { 1045.0, 6947.0 },
                    new List<double> { 1.11, 1.03 }),
                new RoundingStrictPayBandAuditAdjuster(0.05),
            },
            new List<string>(),
            1.8,
            92466.4);
    }

    public double Compute(StrictPayBandAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictPayBandAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
