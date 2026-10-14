namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate assignment calculation.</summary>
public interface IRateAssignmentStep
{
    string Describe();
    double Adjust(RateAssignment item, double valueValue);
}

public sealed record TitleRateAssignmentAdjuster(Dictionary<string, double> Factors) : IRateAssignmentStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RateAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRateAssignmentAdjuster(double Cap) : IRateAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRateAssignmentAdjuster(double Step) : IRateAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRateAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IRateAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateAssignment item, double valueValue)
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

/// <summary>Runs the rate assignment steps in order and keeps a trace of each.</summary>
public sealed class RateAssignmentEngine(List<IRateAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateAssignmentEngine DefaultRateAssignmentEngine()
    {
        return new RateAssignmentEngine(
            new List<IRateAssignmentStep>
            {
                new TitleRateAssignmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.39, ["online"] = 0.86 }),
                new CapRateAssignmentAdjuster(48945.0),
                new RoundingRateAssignmentAdjuster(0.05),
                new TieredRateAssignmentAdjuster(
                    new List<double> { 3111.0, 5629.0, 9369.0 },
                    new List<double> { 0.96, 0.95, 0.83 }),
            },
            new List<string>(),
            1.5,
            99005.3);
    }

    public double Price(RateAssignment item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RateAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
