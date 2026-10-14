namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model reservation calculation.</summary>
public interface ICostModelReservationFactor
{
    string Describe();
    double Adjust(CostModelReservation item, double valueValue);
}

public sealed record RoundingCostModelReservationAdjuster(double Step) : ICostModelReservationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCostModelReservationAdjuster(double Minimum) : ICostModelReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCostModelReservationAdjuster(Registry Flags, string Flag, double Percent) : ICostModelReservationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCostModelReservationAdjuster(double Cap) : ICostModelReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CostModelReservationBatchSizeOffsetAdjuster(double Weight) : ICostModelReservationFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(CostModelReservation item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the cost model reservation steps in order and keeps a trace of each.</summary>
public sealed class CostModelReservationEngine(List<ICostModelReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelReservationEngine DefaultCostModelReservationEngine(Registry flags)
    {
        return new CostModelReservationEngine(
            new List<ICostModelReservationFactor>
            {
                new RoundingCostModelReservationAdjuster(5.0),
                new MinimumCostModelReservationAdjuster(29.3),
                new FlaggedCostModelReservationAdjuster(flags, "cost-model-reservation-global", -3.5),
                new CapCostModelReservationAdjuster(49630.0),
                new CostModelReservationBatchSizeOffsetAdjuster(0.19),
            },
            new List<string>(),
            3.0,
            54393.3);
    }

    public double Price(CostModelReservation item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
