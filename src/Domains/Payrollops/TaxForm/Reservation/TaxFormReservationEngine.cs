namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tax form reservation calculation.</summary>
public interface ITaxFormReservationStep
{
    string Describe();
    double Adjust(TaxFormReservation item, double valueValue);
}

public sealed record RegionTaxFormReservationAdjuster(Dictionary<string, double> Factors) : ITaxFormReservationStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TaxFormReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TaxFormReservationCeilingOffsetAdjuster(double Weight) : ITaxFormReservationStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(TaxFormReservation item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record TieredTaxFormReservationAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormReservation item, double valueValue)
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

public sealed record FlaggedTaxFormReservationAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTaxFormReservationAdjuster(double Cap) : ITaxFormReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTaxFormReservationAdjuster(double Minimum) : ITaxFormReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tax form reservation steps in order and keeps a trace of each.</summary>
public sealed class TaxFormReservationEngine(List<ITaxFormReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormReservationEngine DefaultTaxFormReservationEngine(Registry flags)
    {
        return new TaxFormReservationEngine(
            new List<ITaxFormReservationStep>
            {
                new RegionTaxFormReservationAdjuster(new Dictionary<string, double> { ["west"] = 1.01 }),
                new TaxFormReservationCeilingOffsetAdjuster(1.4),
                new TieredTaxFormReservationAdjuster(
                    new List<double> { 1847.0, 2324.0 },
                    new List<double> { 0.98, 1.19 }),
                new FlaggedTaxFormReservationAdjuster(flags, "tax-form-reservation-default", -5.6),
                new CapTaxFormReservationAdjuster(39389.0),
                new MinimumTaxFormReservationAdjuster(23.1),
            },
            new List<string>(),
            3.5,
            26254.3);
    }

    public double Run(TaxFormReservation item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
