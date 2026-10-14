namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient nomination request calculation.</summary>
public interface ILenientNominationRequestStep
{
    string Describe();
    double Adjust(LenientNominationRequest item, double valueValue);
}

public sealed record RoundingLenientNominationRequestAdjuster(double Step) : ILenientNominationRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientNominationRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLenientNominationRequestAdjuster(double Percent) : ILenientNominationRequestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientNominationRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientNominationRequestAdjuster(double Cap) : ILenientNominationRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientNominationRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient nomination request steps in order and keeps a trace of each.</summary>
public sealed class LenientNominationRequestEngine(List<ILenientNominationRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientNominationRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientNominationRequestEngine DefaultLenientNominationRequestEngine()
    {
        return new LenientNominationRequestEngine(
            new List<ILenientNominationRequestStep>
            {
                new RoundingLenientNominationRequestAdjuster(1.0),
                new PercentLenientNominationRequestAdjuster(17.0),
                new CapLenientNominationRequestAdjuster(23113.0),
            },
            new List<string>(),
            0.1,
            64772.1);
    }

    public double Evaluate(LenientNominationRequest item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientNominationRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
