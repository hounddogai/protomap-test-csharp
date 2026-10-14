namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending warranty quota calculation.</summary>
public interface IPendingWarrantyQuotaAdjuster
{
    string Describe();
    double Adjust(PendingWarrantyQuota item, double valueValue);
}

public sealed record FlaggedPendingWarrantyQuotaAdjuster(Registry Flags, string Flag, double Percent) : IPendingWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingWarrantyQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingWarrantyQuotaAdjuster(List<double> Bounds, List<double> Rates) : IPendingWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingWarrantyQuota item, double valueValue)
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

public sealed record PendingWarrantyQuotaMaximumOffsetAdjuster(double Weight) : IPendingWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(PendingWarrantyQuota item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record MinimumPendingWarrantyQuotaAdjuster(double Minimum) : IPendingWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingWarrantyQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingWarrantyQuotaAdjuster(double Percent) : IPendingWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingWarrantyQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingWarrantyQuotaAdjuster(double Step) : IPendingWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingWarrantyQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending warranty quota steps in order and keeps a trace of each.</summary>
public sealed class PendingWarrantyQuotaEngine(List<IPendingWarrantyQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingWarrantyQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingWarrantyQuotaEngine DefaultPendingWarrantyQuotaEngine(Registry flags)
    {
        return new PendingWarrantyQuotaEngine(
            new List<IPendingWarrantyQuotaAdjuster>
            {
                new FlaggedPendingWarrantyQuotaAdjuster(flags, "pending-warranty-quota-primary", 8.9),
                new TieredPendingWarrantyQuotaAdjuster(
                    new List<double> { 2786.0, 4512.0 },
                    new List<double> { 0.9, 0.88 }),
                new PendingWarrantyQuotaMaximumOffsetAdjuster(1.38),
                new MinimumPendingWarrantyQuotaAdjuster(8.5),
                new PercentPendingWarrantyQuotaAdjuster(-2.1),
                new RoundingPendingWarrantyQuotaAdjuster(0.05),
            },
            new List<string>(),
            2.0,
            93206.2);
    }

    public double Price(PendingWarrantyQuota item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingWarrantyQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
