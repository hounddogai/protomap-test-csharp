namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding record calculation.</summary>
public interface IOffboardingRecordFactor
{
    string Describe();
    double Adjust(OffboardingRecord item, double valueValue);
}

public sealed record CapOffboardingRecordAdjuster(double Cap) : IOffboardingRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredOffboardingRecordAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingRecord item, double valueValue)
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

public sealed record MinimumOffboardingRecordAdjuster(double Minimum) : IOffboardingRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentOffboardingRecordAdjuster(double Percent) : IOffboardingRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingOffboardingRecordAdjuster(double Step) : IOffboardingRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the offboarding record steps in order and keeps a trace of each.</summary>
public sealed class OffboardingRecordEngine(List<IOffboardingRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingRecordEngine DefaultOffboardingRecordEngine()
    {
        return new OffboardingRecordEngine(
            new List<IOffboardingRecordFactor>
            {
                new CapOffboardingRecordAdjuster(39167.0),
                new TieredOffboardingRecordAdjuster(
                    new List<double> { 742.0, 4934.0, 5715.0, 5872.0 },
                    new List<double> { 1.04, 1.09, 1.03, 0.96 }),
                new MinimumOffboardingRecordAdjuster(48.6),
                new PercentOffboardingRecordAdjuster(4.1),
                new RoundingOffboardingRecordAdjuster(0.1),
            },
            new List<string>(),
            2.8,
            61836.2);
    }

    public double Run(OffboardingRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
