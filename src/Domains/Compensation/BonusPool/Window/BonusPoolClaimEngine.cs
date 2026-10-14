namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool claim calculation.</summary>
public interface IBonusPoolClaimFactor
{
    string Describe();
    double Adjust(BonusPoolClaim item, double valueValue);
}

public sealed record CapBonusPoolClaimAdjuster(double Cap) : IBonusPoolClaimFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumBonusPoolClaimAdjuster(double Minimum) : IBonusPoolClaimFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingBonusPoolClaimAdjuster(double Step) : IBonusPoolClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the bonus pool claim steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolClaimEngine(List<IBonusPoolClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolClaimEngine DefaultBonusPoolClaimEngine()
    {
        return new BonusPoolClaimEngine(
            new List<IBonusPoolClaimFactor>
            {
                new CapBonusPoolClaimAdjuster(39226.0),
                new MinimumBonusPoolClaimAdjuster(48.4),
                new RoundingBonusPoolClaimAdjuster(5.0),
            },
            new List<string>(),
            3.4,
            92615.5);
    }

    public double Run(BonusPoolClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
