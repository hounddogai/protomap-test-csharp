namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>One step of the lenient license seat audit calculation.</summary>
public interface ILenientLicenseSeatAuditModifier
{
    string Describe();
    double Adjust(LenientLicenseSeatAudit item, double valueValue);
}

public sealed record TieredLenientLicenseSeatAuditAdjuster(List<double> Bounds, List<double> Rates) : ILenientLicenseSeatAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientLicenseSeatAudit item, double valueValue)
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

public sealed record SourceLenientLicenseSeatAuditAdjuster(Dictionary<string, double> Factors) : ILenientLicenseSeatAuditModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LenientLicenseSeatAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLenientLicenseSeatAuditAdjuster(double Percent) : ILenientLicenseSeatAuditModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientLicenseSeatAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientLicenseSeatAuditAdjuster(double Minimum) : ILenientLicenseSeatAuditModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientLicenseSeatAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient license seat audit steps in order and keeps a trace of each.</summary>
public sealed class LenientLicenseSeatAuditEngine(List<ILenientLicenseSeatAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientLicenseSeatAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientLicenseSeatAuditEngine DefaultLenientLicenseSeatAuditEngine()
    {
        return new LenientLicenseSeatAuditEngine(
            new List<ILenientLicenseSeatAuditModifier>
            {
                new TieredLenientLicenseSeatAuditAdjuster(
                    new List<double> { 8075.0, 9208.0 },
                    new List<double> { 1.08, 0.9 }),
                new SourceLenientLicenseSeatAuditAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.83, ["retail"] = 1.07 }),
                new PercentLenientLicenseSeatAuditAdjuster(16.2),
                new MinimumLenientLicenseSeatAuditAdjuster(47.5),
            },
            new List<string>(),
            1.7,
            34171.7);
    }

    public double Price(LenientLicenseSeatAudit item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientLicenseSeatAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
