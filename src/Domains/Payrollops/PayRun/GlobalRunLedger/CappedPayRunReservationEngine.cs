namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped pay run reservation calculation.</summary>
public interface ICappedPayRunReservationStep
{
    string Describe();
    double Adjust(CappedPayRunReservation item, double valueValue);
}

public sealed record RegionCappedPayRunReservationAdjuster(Dictionary<string, double> Factors) : ICappedPayRunReservationStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CappedPayRunReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCappedPayRunReservationAdjuster(double Percent) : ICappedPayRunReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedPayRunReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedPayRunReservationAdjuster(List<double> Bounds, List<double> Rates) : ICappedPayRunReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPayRunReservation item, double valueValue)
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

public sealed record CappedPayRunReservationFloorValueOffsetAdjuster(double Weight) : ICappedPayRunReservationStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(CappedPayRunReservation item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapCappedPayRunReservationAdjuster(double Cap) : ICappedPayRunReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedPayRunReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCappedPayRunReservationAdjuster(double Step) : ICappedPayRunReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedPayRunReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped pay run reservation steps in order and keeps a trace of each.</summary>
public sealed class CappedPayRunReservationEngine(List<ICappedPayRunReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPayRunReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPayRunReservationEngine DefaultCappedPayRunReservationEngine()
    {
        return new CappedPayRunReservationEngine(
            new List<ICappedPayRunReservationStep>
            {
                new RegionCappedPayRunReservationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.74, ["north"] = 1.07 }),
                new PercentCappedPayRunReservationAdjuster(-13.3),
                new TieredCappedPayRunReservationAdjuster(
                    new List<double> { 1067.0, 5033.0, 9584.0 },
                    new List<double> { 0.98, 1.08, 1.19 }),
                new CappedPayRunReservationFloorValueOffsetAdjuster(0.15),
                new CapCappedPayRunReservationAdjuster(17712.0),
                new RoundingCappedPayRunReservationAdjuster(0.5),
            },
            new List<string>(),
            3.1,
            71295.9);
    }

    public double Price(CappedPayRunReservation item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPayRunReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
