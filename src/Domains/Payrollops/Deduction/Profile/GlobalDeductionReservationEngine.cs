namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global deduction reservation calculation.</summary>
public interface IGlobalDeductionReservationModifier
{
    string Describe();
    double Adjust(GlobalDeductionReservation item, double valueValue);
}

public sealed record FlaggedGlobalDeductionReservationAdjuster(Registry Flags, string Flag, double Percent) : IGlobalDeductionReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalDeductionReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalDeductionReservationAdjuster(double Minimum) : IGlobalDeductionReservationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalDeductionReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GlobalDeductionReservationScoreOffsetAdjuster(double Weight) : IGlobalDeductionReservationModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(GlobalDeductionReservation item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record TieredGlobalDeductionReservationAdjuster(List<double> Bounds, List<double> Rates) : IGlobalDeductionReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalDeductionReservation item, double valueValue)
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

public sealed record PercentGlobalDeductionReservationAdjuster(double Percent) : IGlobalDeductionReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalDeductionReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global deduction reservation steps in order and keeps a trace of each.</summary>
public sealed class GlobalDeductionReservationEngine(List<IGlobalDeductionReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalDeductionReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalDeductionReservationEngine DefaultGlobalDeductionReservationEngine(Registry flags)
    {
        return new GlobalDeductionReservationEngine(
            new List<IGlobalDeductionReservationModifier>
            {
                new FlaggedGlobalDeductionReservationAdjuster(flags, "global-deduction-reservation-seasonal", -3.0),
                new MinimumGlobalDeductionReservationAdjuster(33.9),
                new GlobalDeductionReservationScoreOffsetAdjuster(1.04),
                new TieredGlobalDeductionReservationAdjuster(
                    new List<double> { 6162.0, 8112.0, 8115.0, 8377.0 },
                    new List<double> { 0.93, 0.88, 1.06, 1.1 }),
                new PercentGlobalDeductionReservationAdjuster(-7.4),
            },
            new List<string>(),
            3.1,
            99848.1);
    }

    public double Evaluate(GlobalDeductionReservation item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalDeductionReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
