namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay band exception calculation.</summary>
public interface IPayBandExceptionAdjuster
{
    string Describe();
    double Adjust(PayBandException item, double valueValue);
}

public sealed record PercentPayBandExceptionAdjuster(double Percent) : IPayBandExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayBandExceptionAdjuster(List<double> Bounds, List<double> Rates) : IPayBandExceptionAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandException item, double valueValue)
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

public sealed record PayBandExceptionWeightOffsetAdjuster(double Weight) : IPayBandExceptionAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(PayBandException item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record FlaggedPayBandExceptionAdjuster(Registry Flags, string Flag, double Percent) : IPayBandExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePayBandExceptionAdjuster(Dictionary<string, double> Factors) : IPayBandExceptionAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PayBandException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay band exception steps in order and keeps a trace of each.</summary>
public sealed class PayBandExceptionEngine(List<IPayBandExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandExceptionEngine DefaultPayBandExceptionEngine(Registry flags)
    {
        return new PayBandExceptionEngine(
            new List<IPayBandExceptionAdjuster>
            {
                new PercentPayBandExceptionAdjuster(14.0),
                new TieredPayBandExceptionAdjuster(
                    new List<double> { 1187.0, 4221.0 },
                    new List<double> { 1.17, 1.02 }),
                new PayBandExceptionWeightOffsetAdjuster(1.3),
                new FlaggedPayBandExceptionAdjuster(flags, "pay-band-exception-lenient", -7.6),
                new NamePayBandExceptionAdjuster(new Dictionary<string, double> { ["retail"] = 0.97 }),
            },
            new List<string>(),
            2.1,
            28274.5);
    }

    public double Run(PayBandException item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
