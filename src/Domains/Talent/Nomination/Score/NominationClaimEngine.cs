namespace Acme.Workforce.Talent.Nomination.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination claim calculation.</summary>
public interface INominationClaimModifier
{
    string Describe();
    double Adjust(NominationClaim item, double valueValue);
}

public sealed record MinimumNominationClaimAdjuster(double Minimum) : INominationClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NominationClaimAttemptsOffsetAdjuster(double Weight) : INominationClaimModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(NominationClaim item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record CapNominationClaimAdjuster(double Cap) : INominationClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingNominationClaimAdjuster(double Step) : INominationClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the nomination claim steps in order and keeps a trace of each.</summary>
public sealed class NominationClaimEngine(List<INominationClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationClaimEngine DefaultNominationClaimEngine()
    {
        return new NominationClaimEngine(
            new List<INominationClaimModifier>
            {
                new MinimumNominationClaimAdjuster(23.0),
                new NominationClaimAttemptsOffsetAdjuster(1.21),
                new CapNominationClaimAdjuster(37983.0),
                new RoundingNominationClaimAdjuster(5.0),
            },
            new List<string>(),
            1.5,
            47386.0);
    }

    public double Price(NominationClaim item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<NominationClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
