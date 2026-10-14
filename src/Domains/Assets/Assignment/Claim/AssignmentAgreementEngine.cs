namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment agreement calculation.</summary>
public interface IAssignmentAgreementFactor
{
    string Describe();
    double Adjust(AssignmentAgreement item, double valueValue);
}

public sealed record MinimumAssignmentAgreementAdjuster(double Minimum) : IAssignmentAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAssignmentAgreementAdjuster(double Step) : IAssignmentAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAssignmentAgreementAdjuster(double Cap) : IAssignmentAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleAssignmentAgreementAdjuster(Dictionary<string, double> Factors) : IAssignmentAgreementFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AssignmentAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAssignmentAgreementAdjuster(double Percent) : IAssignmentAgreementFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the assignment agreement steps in order and keeps a trace of each.</summary>
public sealed class AssignmentAgreementEngine(List<IAssignmentAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentAgreementEngine DefaultAssignmentAgreementEngine()
    {
        return new AssignmentAgreementEngine(
            new List<IAssignmentAgreementFactor>
            {
                new MinimumAssignmentAgreementAdjuster(26.0),
                new RoundingAssignmentAgreementAdjuster(0.05),
                new CapAssignmentAgreementAdjuster(3647.0),
                new TitleAssignmentAgreementAdjuster(new Dictionary<string, double> { ["east"] = 1.04 }),
                new PercentAssignmentAgreementAdjuster(-24.3),
            },
            new List<string>(),
            2.5,
            69309.7);
    }

    public double Compute(AssignmentAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
