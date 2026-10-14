namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant baseline calculation.</summary>
public interface IEquityGrantBaselineStep
{
    string Describe();
    double Adjust(EquityGrantBaseline item, double valueValue);
}

public sealed record RoundingEquityGrantBaselineAdjuster(double Step) : IEquityGrantBaselineStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredEquityGrantBaselineAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantBaselineStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantBaseline item, double valueValue)
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

public sealed record FlaggedEquityGrantBaselineAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantBaselineStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record EquityGrantBaselineLeaveDaysOffsetAdjuster(double Weight) : IEquityGrantBaselineStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(EquityGrantBaseline item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record MinimumEquityGrantBaselineAdjuster(double Minimum) : IEquityGrantBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the equity grant baseline steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantBaselineEngine(List<IEquityGrantBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantBaselineEngine DefaultEquityGrantBaselineEngine(Registry flags)
    {
        return new EquityGrantBaselineEngine(
            new List<IEquityGrantBaselineStep>
            {
                new RoundingEquityGrantBaselineAdjuster(1.0),
                new TieredEquityGrantBaselineAdjuster(
                    new List<double> { 5601.0, 6922.0, 9173.0 },
                    new List<double> { 0.86, 0.88, 1.2 }),
                new FlaggedEquityGrantBaselineAdjuster(flags, "equity-grant-baseline-capped", -2.4),
                new EquityGrantBaselineLeaveDaysOffsetAdjuster(0.34),
                new MinimumEquityGrantBaselineAdjuster(49.7),
            },
            new List<string>(),
            2.6,
            67818.1);
    }

    public double Compute(EquityGrantBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
