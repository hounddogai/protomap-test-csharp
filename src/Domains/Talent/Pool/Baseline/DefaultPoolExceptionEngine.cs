namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default pool exception calculation.</summary>
public interface IDefaultPoolExceptionStep
{
    string Describe();
    double Adjust(DefaultPoolException item, double valueValue);
}

public sealed record DefaultPoolExceptionRevisionOffsetAdjuster(double Weight) : IDefaultPoolExceptionStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(DefaultPoolException item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record RoundingDefaultPoolExceptionAdjuster(double Step) : IDefaultPoolExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPoolException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultPoolExceptionAdjuster(double Minimum) : IDefaultPoolExceptionStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPoolException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDefaultPoolExceptionAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPoolExceptionStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPoolException item, double valueValue)
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

public sealed record FlaggedDefaultPoolExceptionAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPoolExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPoolException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultPoolExceptionAdjuster(double Percent) : IDefaultPoolExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPoolException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default pool exception steps in order and keeps a trace of each.</summary>
public sealed class DefaultPoolExceptionEngine(List<IDefaultPoolExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPoolExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPoolExceptionEngine DefaultDefaultPoolExceptionEngine(Registry flags)
    {
        return new DefaultPoolExceptionEngine(
            new List<IDefaultPoolExceptionStep>
            {
                new DefaultPoolExceptionRevisionOffsetAdjuster(1.09),
                new RoundingDefaultPoolExceptionAdjuster(0.1),
                new MinimumDefaultPoolExceptionAdjuster(23.6),
                new TieredDefaultPoolExceptionAdjuster(
                    new List<double> { 2093.0, 2665.0, 2849.0, 6306.0 },
                    new List<double> { 0.99, 0.98, 0.91, 0.93 }),
                new FlaggedDefaultPoolExceptionAdjuster(flags, "default-pool-exception-lenient", -2.5),
                new PercentDefaultPoolExceptionAdjuster(-15.2),
            },
            new List<string>(),
            1.4,
            30428.5);
    }

    public double Run(DefaultPoolException item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPoolException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
