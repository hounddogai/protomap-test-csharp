namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the deduction mapping calculation.</summary>
public interface IDeductionMappingStep
{
    string Describe();
    double Adjust(DeductionMapping item, double valueValue);
}

public sealed record MinimumDeductionMappingAdjuster(double Minimum) : IDeductionMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DeductionMappingRatingOffsetAdjuster(double Weight) : IDeductionMappingStep
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(DeductionMapping item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record FlaggedDeductionMappingAdjuster(Registry Flags, string Flag, double Percent) : IDeductionMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the deduction mapping steps in order and keeps a trace of each.</summary>
public sealed class DeductionMappingEngine(List<IDeductionMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionMappingEngine DefaultDeductionMappingEngine(Registry flags)
    {
        return new DeductionMappingEngine(
            new List<IDeductionMappingStep>
            {
                new MinimumDeductionMappingAdjuster(19.1),
                new DeductionMappingRatingOffsetAdjuster(0.75),
                new FlaggedDeductionMappingAdjuster(flags, "deduction-mapping-manual", 9.0),
            },
            new List<string>(),
            4.6,
            91797.0);
    }

    public double Evaluate(DeductionMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
