namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default exception quota calculation.</summary>
public interface IDefaultExceptionQuotaFactor
{
    string Describe();
    double Adjust(DefaultExceptionQuota item, double valueValue);
}

public sealed record CapDefaultExceptionQuotaAdjuster(double Cap) : IDefaultExceptionQuotaFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultExceptionQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDefaultExceptionQuotaAdjuster(List<double> Bounds, List<double> Rates) : IDefaultExceptionQuotaFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultExceptionQuota item, double valueValue)
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

public sealed record DefaultExceptionQuotaCostOffsetAdjuster(double Weight) : IDefaultExceptionQuotaFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(DefaultExceptionQuota item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record MinimumDefaultExceptionQuotaAdjuster(double Minimum) : IDefaultExceptionQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultExceptionQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CountryDefaultExceptionQuotaAdjuster(Dictionary<string, double> Factors) : IDefaultExceptionQuotaFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DefaultExceptionQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedDefaultExceptionQuotaAdjuster(Registry Flags, string Flag, double Percent) : IDefaultExceptionQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultExceptionQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default exception quota steps in order and keeps a trace of each.</summary>
public sealed class DefaultExceptionQuotaEngine(List<IDefaultExceptionQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultExceptionQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultExceptionQuotaEngine DefaultDefaultExceptionQuotaEngine(Registry flags)
    {
        return new DefaultExceptionQuotaEngine(
            new List<IDefaultExceptionQuotaFactor>
            {
                new CapDefaultExceptionQuotaAdjuster(32511.0),
                new TieredDefaultExceptionQuotaAdjuster(
                    new List<double> { 1616.0, 6138.0, 6948.0 },
                    new List<double> { 0.83, 0.99, 0.97 }),
                new DefaultExceptionQuotaCostOffsetAdjuster(1.14),
                new MinimumDefaultExceptionQuotaAdjuster(8.7),
                new CountryDefaultExceptionQuotaAdjuster(new Dictionary<string, double> { ["west"] = 1.29 }),
                new FlaggedDefaultExceptionQuotaAdjuster(flags, "default-exception-quota-seasonal", -9.1),
            },
            new List<string>(),
            2.7,
            11324.7);
    }

    public double Evaluate(DefaultExceptionQuota item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultExceptionQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
