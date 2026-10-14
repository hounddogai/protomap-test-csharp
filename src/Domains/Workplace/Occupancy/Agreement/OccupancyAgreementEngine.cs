namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy agreement calculation.</summary>
public interface IOccupancyAgreementAdjuster
{
    string Describe();
    double Adjust(OccupancyAgreement item, double valueValue);
}

public sealed record OccupancyAgreementLimitOffsetAdjuster(double Weight) : IOccupancyAgreementAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(OccupancyAgreement item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record MinimumOccupancyAgreementAdjuster(double Minimum) : IOccupancyAgreementAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredOccupancyAgreementAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyAgreement item, double valueValue)
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

public sealed record RoundingOccupancyAgreementAdjuster(double Step) : IOccupancyAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the occupancy agreement steps in order and keeps a trace of each.</summary>
public sealed class OccupancyAgreementEngine(List<IOccupancyAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyAgreementEngine DefaultOccupancyAgreementEngine()
    {
        return new OccupancyAgreementEngine(
            new List<IOccupancyAgreementAdjuster>
            {
                new OccupancyAgreementLimitOffsetAdjuster(1.05),
                new MinimumOccupancyAgreementAdjuster(18.4),
                new TieredOccupancyAgreementAdjuster(
                    new List<double> { 3704.0, 4045.0, 8799.0, 9875.0 },
                    new List<double> { 1.03, 1.13, 0.82, 1.07 }),
                new RoundingOccupancyAgreementAdjuster(0.5),
            },
            new List<string>(),
            3.8,
            30283.5);
    }

    public double Compute(OccupancyAgreement item)
    {
        var current = item.WeightedExposure();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<OccupancyAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
