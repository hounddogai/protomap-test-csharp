namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default booking claim calculation.</summary>
public interface IDefaultBookingClaimAdjuster
{
    string Describe();
    double Adjust(DefaultBookingClaim item, double valueValue);
}

public sealed record CapDefaultBookingClaimAdjuster(double Cap) : IDefaultBookingClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultBookingClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionDefaultBookingClaimAdjuster(Dictionary<string, double> Factors) : IDefaultBookingClaimAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(DefaultBookingClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDefaultBookingClaimAdjuster(double Minimum) : IDefaultBookingClaimAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultBookingClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingDefaultBookingClaimAdjuster(double Step) : IDefaultBookingClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultBookingClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default booking claim steps in order and keeps a trace of each.</summary>
public sealed class DefaultBookingClaimEngine(List<IDefaultBookingClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultBookingClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultBookingClaimEngine DefaultDefaultBookingClaimEngine()
    {
        return new DefaultBookingClaimEngine(
            new List<IDefaultBookingClaimAdjuster>
            {
                new CapDefaultBookingClaimAdjuster(30885.0),
                new RegionDefaultBookingClaimAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.24, ["online"] = 0.76, ["west"] = 0.76 }),
                new MinimumDefaultBookingClaimAdjuster(15.9),
                new RoundingDefaultBookingClaimAdjuster(0.5),
            },
            new List<string>(),
            2.7,
            17875.1);
    }

    public double Price(DefaultBookingClaim item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultBookingClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
