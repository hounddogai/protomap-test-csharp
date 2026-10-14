namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped scenario reservation calculation.</summary>
public interface ICappedScenarioReservationFactor
{
    string Describe();
    double Adjust(CappedScenarioReservation item, double valueValue);
}

public sealed record FlaggedCappedScenarioReservationAdjuster(Registry Flags, string Flag, double Percent) : ICappedScenarioReservationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedScenarioReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCappedScenarioReservationAdjuster(double Minimum) : ICappedScenarioReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedScenarioReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CappedScenarioReservationTenureMonthsOffsetAdjuster(double Weight) : ICappedScenarioReservationFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(CappedScenarioReservation item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record RoundingCappedScenarioReservationAdjuster(double Step) : ICappedScenarioReservationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedScenarioReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedScenarioReservationAdjuster(List<double> Bounds, List<double> Rates) : ICappedScenarioReservationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedScenarioReservation item, double valueValue)
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

/// <summary>Runs the capped scenario reservation steps in order and keeps a trace of each.</summary>
public sealed class CappedScenarioReservationEngine(List<ICappedScenarioReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedScenarioReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedScenarioReservationEngine DefaultCappedScenarioReservationEngine(Registry flags)
    {
        return new CappedScenarioReservationEngine(
            new List<ICappedScenarioReservationFactor>
            {
                new FlaggedCappedScenarioReservationAdjuster(flags, "capped-scenario-reservation-batched", 6.4),
                new MinimumCappedScenarioReservationAdjuster(14.7),
                new CappedScenarioReservationTenureMonthsOffsetAdjuster(1.48),
                new RoundingCappedScenarioReservationAdjuster(0.05),
                new TieredCappedScenarioReservationAdjuster(
                    new List<double> { 727.0, 2699.0, 6532.0 },
                    new List<double> { 0.93, 0.83, 0.98 }),
            },
            new List<string>(),
            4.8,
            98196.5);
    }

    public double Evaluate(CappedScenarioReservation item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedScenarioReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
