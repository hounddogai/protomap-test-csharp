namespace Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>One step of the capped nomination assignment calculation.</summary>
public interface ICappedNominationAssignmentModifier
{
    string Describe();
    double Adjust(CappedNominationAssignment item, double valueValue);
}

public sealed record MinimumCappedNominationAssignmentAdjuster(double Minimum) : ICappedNominationAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedNominationAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CappedNominationAssignmentRatioOffsetAdjuster(double Weight) : ICappedNominationAssignmentModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(CappedNominationAssignment item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CapCappedNominationAssignmentAdjuster(double Cap) : ICappedNominationAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedNominationAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped nomination assignment steps in order and keeps a trace of each.</summary>
public sealed class CappedNominationAssignmentEngine(List<ICappedNominationAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedNominationAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedNominationAssignmentEngine DefaultCappedNominationAssignmentEngine()
    {
        return new CappedNominationAssignmentEngine(
            new List<ICappedNominationAssignmentModifier>
            {
                new MinimumCappedNominationAssignmentAdjuster(39.1),
                new CappedNominationAssignmentRatioOffsetAdjuster(1.49),
                new CapCappedNominationAssignmentAdjuster(35846.0),
            },
            new List<string>(),
            2.2,
            48905.8);
    }

    public double Price(CappedNominationAssignment item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedNominationAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
