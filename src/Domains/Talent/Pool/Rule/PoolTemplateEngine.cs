namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool template calculation.</summary>
public interface IPoolTemplateStep
{
    string Describe();
    double Adjust(PoolTemplate item, double valueValue);
}

public sealed record PoolTemplateAmountOffsetAdjuster(double Weight) : IPoolTemplateStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(PoolTemplate item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record FlaggedPoolTemplateAdjuster(Registry Flags, string Flag, double Percent) : IPoolTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPoolTemplateAdjuster(double Step) : IPoolTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPoolTemplateAdjuster(double Minimum) : IPoolTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pool template steps in order and keeps a trace of each.</summary>
public sealed class PoolTemplateEngine(List<IPoolTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolTemplateEngine DefaultPoolTemplateEngine(Registry flags)
    {
        return new PoolTemplateEngine(
            new List<IPoolTemplateStep>
            {
                new PoolTemplateAmountOffsetAdjuster(1.18),
                new FlaggedPoolTemplateAdjuster(flags, "pool-template-manual", 2.8),
                new RoundingPoolTemplateAdjuster(1.0),
                new MinimumPoolTemplateAdjuster(16.4),
            },
            new List<string>(),
            4.2,
            42513.6);
    }

    public double Evaluate(PoolTemplate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PoolTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
