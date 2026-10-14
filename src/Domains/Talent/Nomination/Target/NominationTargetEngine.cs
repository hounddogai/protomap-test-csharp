namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination target calculation.</summary>
public interface INominationTargetModifier
{
    string Describe();
    double Adjust(NominationTarget item, double valueValue);
}

public sealed record CapNominationTargetAdjuster(double Cap) : INominationTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NominationTargetWeightOffsetAdjuster(double Weight) : INominationTargetModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(NominationTarget item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingNominationTargetAdjuster(double Step) : INominationTargetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameNominationTargetAdjuster(Dictionary<string, double> Factors) : INominationTargetModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(NominationTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the nomination target steps in order and keeps a trace of each.</summary>
public sealed class NominationTargetEngine(List<INominationTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationTargetEngine DefaultNominationTargetEngine()
    {
        return new NominationTargetEngine(
            new List<INominationTargetModifier>
            {
                new CapNominationTargetAdjuster(45008.0),
                new NominationTargetWeightOffsetAdjuster(1.99),
                new RoundingNominationTargetAdjuster(0.05),
                new NameNominationTargetAdjuster(new Dictionary<string, double> { ["east"] = 0.72 }),
            },
            new List<string>(),
            1.0,
            88820.5);
    }

    public double Price(NominationTarget item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<NominationTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
