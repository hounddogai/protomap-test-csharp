namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility assignment calculation.</summary>
public interface IMobilityAssignmentModifier
{
    string Describe();
    double Adjust(MobilityAssignment item, double valueValue);
}

public sealed record RoundingMobilityAssignmentAdjuster(double Step) : IMobilityAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MobilityAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapMobilityAssignmentAdjuster(double Cap) : IMobilityAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumMobilityAssignmentAdjuster(double Minimum) : IMobilityAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record MobilityAssignmentDiscountOffsetAdjuster(double Weight) : IMobilityAssignmentModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(MobilityAssignment item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the mobility assignment steps in order and keeps a trace of each.</summary>
public sealed class MobilityAssignmentEngine(List<IMobilityAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityAssignmentEngine DefaultMobilityAssignmentEngine()
    {
        return new MobilityAssignmentEngine(
            new List<IMobilityAssignmentModifier>
            {
                new RoundingMobilityAssignmentAdjuster(1.0),
                new CapMobilityAssignmentAdjuster(19253.0),
                new MinimumMobilityAssignmentAdjuster(14.9),
                new MobilityAssignmentDiscountOffsetAdjuster(1.57),
            },
            new List<string>(),
            4.4,
            62928.5);
    }

    public double Run(MobilityAssignment item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
