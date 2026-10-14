namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack reservation calculation.</summary>
public interface IPolicyAckReservationFactor
{
    string Describe();
    double Adjust(PolicyAckReservation item, double valueValue);
}

public sealed record RoundingPolicyAckReservationAdjuster(double Step) : IPolicyAckReservationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPolicyAckReservationAdjuster(double Minimum) : IPolicyAckReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPolicyAckReservationAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckReservationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckReservation item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record TitlePolicyAckReservationAdjuster(Dictionary<string, double> Factors) : IPolicyAckReservationFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PolicyAckReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyAckReservationAdjuster(double Cap) : IPolicyAckReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy ack reservation steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckReservationEngine(List<IPolicyAckReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckReservationEngine DefaultPolicyAckReservationEngine()
    {
        return new PolicyAckReservationEngine(
            new List<IPolicyAckReservationFactor>
            {
                new RoundingPolicyAckReservationAdjuster(0.25),
                new MinimumPolicyAckReservationAdjuster(9.3),
                new TieredPolicyAckReservationAdjuster(
                    new List<double> { 6329.0, 6965.0 },
                    new List<double> { 1.17, 1.08 }),
                new TitlePolicyAckReservationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.37, ["online"] = 1.11, ["central"] = 1.37 }),
                new CapPolicyAckReservationAdjuster(47528.0),
            },
            new List<string>(),
            3.3,
            3739.5);
    }

    public double Price(PolicyAckReservation item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
