namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination adjustment calculation.</summary>
public interface INominationAdjustmentStep
{
    string Describe();
    double Adjust(NominationAdjustment item, double valueValue);
}

public sealed record PercentNominationAdjustmentAdjuster(double Percent) : INominationAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : INominationAdjustmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationAdjustment item, double valueValue)
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

public sealed record RoundingNominationAdjustmentAdjuster(double Step) : INominationAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapNominationAdjustmentAdjuster(double Cap) : INominationAdjustmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumNominationAdjustmentAdjuster(double Minimum) : INominationAdjustmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the nomination adjustment steps in order and keeps a trace of each.</summary>
public sealed class NominationAdjustmentEngine(List<INominationAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationAdjustmentEngine DefaultNominationAdjustmentEngine()
    {
        return new NominationAdjustmentEngine(
            new List<INominationAdjustmentStep>
            {
                new PercentNominationAdjustmentAdjuster(-18.5),
                new TieredNominationAdjustmentAdjuster(
                    new List<double> { 579.0, 1619.0, 5289.0, 8654.0 },
                    new List<double> { 0.88, 0.92, 0.88, 0.98 }),
                new RoundingNominationAdjustmentAdjuster(5.0),
                new CapNominationAdjustmentAdjuster(49615.0),
                new MinimumNominationAdjustmentAdjuster(18.6),
            },
            new List<string>(),
            1.8,
            41707.1);
    }

    public double Run(NominationAdjustment item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<NominationAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
