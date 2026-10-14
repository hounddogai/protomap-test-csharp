namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band policy calculation.</summary>
public interface IPayBandPolicyModifier
{
    string Describe();
    double Adjust(PayBandPolicy item, double valueValue);
}

public sealed record PercentPayBandPolicyAdjuster(double Percent) : IPayBandPolicyModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPayBandPolicyAdjuster(double Step) : IPayBandPolicyModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PayBandPolicyCapacityOffsetAdjuster(double Weight) : IPayBandPolicyModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(PayBandPolicy item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record CapPayBandPolicyAdjuster(double Cap) : IPayBandPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pay band policy steps in order and keeps a trace of each.</summary>
public sealed class PayBandPolicyEngine(List<IPayBandPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandPolicyEngine DefaultPayBandPolicyEngine()
    {
        return new PayBandPolicyEngine(
            new List<IPayBandPolicyModifier>
            {
                new PercentPayBandPolicyAdjuster(-0.3),
                new RoundingPayBandPolicyAdjuster(5.0),
                new PayBandPolicyCapacityOffsetAdjuster(1.0),
                new CapPayBandPolicyAdjuster(26313.0),
            },
            new List<string>(),
            0.3,
            37278.1);
    }

    public double Price(PayBandPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
