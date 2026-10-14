namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default nomination assignment calculation.</summary>
public interface IDefaultNominationAssignmentFactor
{
    string Describe();
    double Adjust(DefaultNominationAssignment item, double valueValue);
}

public sealed record DefaultNominationAssignmentAttemptsOffsetAdjuster(double Weight) : IDefaultNominationAssignmentFactor
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(DefaultNominationAssignment item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record MinimumDefaultNominationAssignmentAdjuster(double Minimum) : IDefaultNominationAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultNominationAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleDefaultNominationAssignmentAdjuster(Dictionary<string, double> Factors) : IDefaultNominationAssignmentFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultNominationAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultNominationAssignmentAdjuster(double Step) : IDefaultNominationAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultNominationAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default nomination assignment steps in order and keeps a trace of each.</summary>
public sealed class DefaultNominationAssignmentEngine(List<IDefaultNominationAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultNominationAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultNominationAssignmentEngine DefaultDefaultNominationAssignmentEngine()
    {
        return new DefaultNominationAssignmentEngine(
            new List<IDefaultNominationAssignmentFactor>
            {
                new DefaultNominationAssignmentAttemptsOffsetAdjuster(0.17),
                new MinimumDefaultNominationAssignmentAdjuster(6.1),
                new TitleDefaultNominationAssignmentAdjuster(new Dictionary<string, double> { ["north"] = 0.98 }),
                new RoundingDefaultNominationAssignmentAdjuster(0.05),
            },
            new List<string>(),
            3.8,
            11371.3);
    }

    public double Price(DefaultNominationAssignment item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultNominationAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
