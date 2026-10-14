namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual balance digest calculation.</summary>
public interface IManualBalanceDigestFactor
{
    string Describe();
    double Adjust(ManualBalanceDigest item, double valueValue);
}

public sealed record CapManualBalanceDigestAdjuster(double Cap) : IManualBalanceDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualBalanceDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualBalanceDigestExpiresAtOffsetAdjuster(double Weight) : IManualBalanceDigestFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(ManualBalanceDigest item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingManualBalanceDigestAdjuster(double Step) : IManualBalanceDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualBalanceDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the manual balance digest steps in order and keeps a trace of each.</summary>
public sealed class ManualBalanceDigestEngine(List<IManualBalanceDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBalanceDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBalanceDigestEngine DefaultManualBalanceDigestEngine()
    {
        return new ManualBalanceDigestEngine(
            new List<IManualBalanceDigestFactor>
            {
                new CapManualBalanceDigestAdjuster(22220.0),
                new ManualBalanceDigestExpiresAtOffsetAdjuster(1.66),
                new RoundingManualBalanceDigestAdjuster(0.1),
            },
            new List<string>(),
            0.3,
            34698.0);
    }

    public double Evaluate(ManualBalanceDigest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualBalanceDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
