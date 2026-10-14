namespace Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>One step of the rolling span exception calculation.</summary>
public interface IRollingSpanExceptionModifier
{
    string Describe();
    double Adjust(RollingSpanException item, double valueValue);
}

public sealed record RollingSpanExceptionAmountOffsetAdjuster(double Weight) : IRollingSpanExceptionModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RollingSpanException item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredRollingSpanExceptionAdjuster(List<double> Bounds, List<double> Rates) : IRollingSpanExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingSpanException item, double valueValue)
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

public sealed record PercentRollingSpanExceptionAdjuster(double Percent) : IRollingSpanExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingSpanException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingSpanExceptionAdjuster(double Minimum) : IRollingSpanExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingSpanException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling span exception steps in order and keeps a trace of each.</summary>
public sealed class RollingSpanExceptionEngine(List<IRollingSpanExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingSpanExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingSpanExceptionEngine DefaultRollingSpanExceptionEngine()
    {
        return new RollingSpanExceptionEngine(
            new List<IRollingSpanExceptionModifier>
            {
                new RollingSpanExceptionAmountOffsetAdjuster(1.67),
                new TieredRollingSpanExceptionAdjuster(
                    new List<double> { 3405.0, 5807.0, 8186.0 },
                    new List<double> { 1.13, 1.06, 0.84 }),
                new PercentRollingSpanExceptionAdjuster(2.0),
                new MinimumRollingSpanExceptionAdjuster(25.1),
            },
            new List<string>(),
            2.8,
            79529.9);
    }

    public double Evaluate(RollingSpanException item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingSpanException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
