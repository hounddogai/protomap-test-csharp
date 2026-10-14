namespace Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>One step of the exception draft calculation.</summary>
public interface IExceptionDraftAdjuster
{
    string Describe();
    double Adjust(ExceptionDraft item, double valueValue);
}

public sealed record PercentExceptionDraftAdjuster(double Percent) : IExceptionDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumExceptionDraftAdjuster(double Minimum) : IExceptionDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapExceptionDraftAdjuster(double Cap) : IExceptionDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record UnitExceptionDraftAdjuster(Dictionary<string, double> Factors) : IExceptionDraftAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ExceptionDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredExceptionDraftAdjuster(List<double> Bounds, List<double> Rates) : IExceptionDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionDraft item, double valueValue)
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

public sealed record ExceptionDraftExpiresAtOffsetAdjuster(double Weight) : IExceptionDraftAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(ExceptionDraft item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the exception draft steps in order and keeps a trace of each.</summary>
public sealed class ExceptionDraftEngine(List<IExceptionDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionDraftEngine DefaultExceptionDraftEngine()
    {
        return new ExceptionDraftEngine(
            new List<IExceptionDraftAdjuster>
            {
                new PercentExceptionDraftAdjuster(14.1),
                new MinimumExceptionDraftAdjuster(4.9),
                new CapExceptionDraftAdjuster(11999.0),
                new UnitExceptionDraftAdjuster(new Dictionary<string, double> { ["east"] = 1.21, ["west"] = 0.82 }),
                new TieredExceptionDraftAdjuster(
                    new List<double> { 5028.0, 7350.0, 9254.0, 9817.0 },
                    new List<double> { 1.08, 1.1, 0.98, 0.93 }),
                new ExceptionDraftExpiresAtOffsetAdjuster(1.31),
            },
            new List<string>(),
            3.0,
            30215.3);
    }

    public double Compute(ExceptionDraft item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
