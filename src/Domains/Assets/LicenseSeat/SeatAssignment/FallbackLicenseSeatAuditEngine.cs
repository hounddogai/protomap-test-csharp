namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>One step of the fallback license seat audit calculation.</summary>
public interface IFallbackLicenseSeatAuditStep
{
    string Describe();
    double Adjust(FallbackLicenseSeatAudit item, double valueValue);
}

public sealed record CapFallbackLicenseSeatAuditAdjuster(double Cap) : IFallbackLicenseSeatAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackLicenseSeatAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumFallbackLicenseSeatAuditAdjuster(double Minimum) : IFallbackLicenseSeatAuditStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackLicenseSeatAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFallbackLicenseSeatAuditAdjuster(double Percent) : IFallbackLicenseSeatAuditStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackLicenseSeatAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryFallbackLicenseSeatAuditAdjuster(Dictionary<string, double> Factors) : IFallbackLicenseSeatAuditStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(FallbackLicenseSeatAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredFallbackLicenseSeatAuditAdjuster(List<double> Bounds, List<double> Rates) : IFallbackLicenseSeatAuditStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackLicenseSeatAudit item, double valueValue)
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

/// <summary>Runs the fallback license seat audit steps in order and keeps a trace of each.</summary>
public sealed class FallbackLicenseSeatAuditEngine(List<IFallbackLicenseSeatAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackLicenseSeatAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackLicenseSeatAuditEngine DefaultFallbackLicenseSeatAuditEngine()
    {
        return new FallbackLicenseSeatAuditEngine(
            new List<IFallbackLicenseSeatAuditStep>
            {
                new CapFallbackLicenseSeatAuditAdjuster(33242.0),
                new MinimumFallbackLicenseSeatAuditAdjuster(36.7),
                new PercentFallbackLicenseSeatAuditAdjuster(12.7),
                new CountryFallbackLicenseSeatAuditAdjuster(new Dictionary<string, double> { ["retail"] = 0.74 }),
                new TieredFallbackLicenseSeatAuditAdjuster(
                    new List<double> { 3102.0, 6379.0, 8215.0, 9467.0 },
                    new List<double> { 1.1, 1.11, 1.19, 0.91 }),
            },
            new List<string>(),
            0.2,
            30535.4);
    }

    public double Evaluate(FallbackLicenseSeatAudit item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackLicenseSeatAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
