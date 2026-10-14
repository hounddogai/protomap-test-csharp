namespace Acme.Workforce.Workplace.Booking.Digest;

/// <summary>One step of the pending booking digest calculation.</summary>
public interface IPendingBookingDigestModifier
{
    string Describe();
    double Adjust(PendingBookingDigest item, double valueValue);
}

public sealed record CategoryPendingBookingDigestAdjuster(Dictionary<string, double> Factors) : IPendingBookingDigestModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PendingBookingDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingBookingDigestAdjuster(double Percent) : IPendingBookingDigestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingBookingDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingBookingDigestAdjuster(List<double> Bounds, List<double> Rates) : IPendingBookingDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingBookingDigest item, double valueValue)
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

public sealed record CapPendingBookingDigestAdjuster(double Cap) : IPendingBookingDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingBookingDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PendingBookingDigestGraceDaysOffsetAdjuster(double Weight) : IPendingBookingDigestModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(PendingBookingDigest item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record MinimumPendingBookingDigestAdjuster(double Minimum) : IPendingBookingDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingBookingDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending booking digest steps in order and keeps a trace of each.</summary>
public sealed class PendingBookingDigestEngine(List<IPendingBookingDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingBookingDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingBookingDigestEngine DefaultPendingBookingDigestEngine()
    {
        return new PendingBookingDigestEngine(
            new List<IPendingBookingDigestModifier>
            {
                new CategoryPendingBookingDigestAdjuster(new Dictionary<string, double> { ["retail"] = 1.29 }),
                new PercentPendingBookingDigestAdjuster(-13.4),
                new TieredPendingBookingDigestAdjuster(
                    new List<double> { 7587.0, 9259.0 },
                    new List<double> { 1.11, 0.86 }),
                new CapPendingBookingDigestAdjuster(5974.0),
                new PendingBookingDigestGraceDaysOffsetAdjuster(1.16),
                new MinimumPendingBookingDigestAdjuster(19.7),
            },
            new List<string>(),
            3.9,
            52303.1);
    }

    public double Run(PendingBookingDigest item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingBookingDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
