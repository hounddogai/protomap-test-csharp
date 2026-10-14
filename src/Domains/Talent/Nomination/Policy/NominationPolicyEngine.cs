namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination policy calculation.</summary>
public interface INominationPolicyModifier
{
    string Describe();
    double Adjust(NominationPolicy item, double valueValue);
}

public sealed record LabelNominationPolicyAdjuster(Dictionary<string, double> Factors) : INominationPolicyModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(NominationPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumNominationPolicyAdjuster(double Minimum) : INominationPolicyModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapNominationPolicyAdjuster(double Cap) : INominationPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingNominationPolicyAdjuster(double Step) : INominationPolicyModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the nomination policy steps in order and keeps a trace of each.</summary>
public sealed class NominationPolicyEngine(List<INominationPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationPolicyEngine DefaultNominationPolicyEngine()
    {
        return new NominationPolicyEngine(
            new List<INominationPolicyModifier>
            {
                new LabelNominationPolicyAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.39, ["west"] = 1.25 }),
                new MinimumNominationPolicyAdjuster(46.0),
                new CapNominationPolicyAdjuster(8117.0),
                new RoundingNominationPolicyAdjuster(1.0),
            },
            new List<string>(),
            0.9,
            85803.3);
    }

    public double Evaluate(NominationPolicy item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<NominationPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
