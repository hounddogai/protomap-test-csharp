namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped statement of work exception calculation.</summary>
public interface ICappedStatementOfWorkExceptionModifier
{
    string Describe();
    double Adjust(CappedStatementOfWorkException item, double valueValue);
}

public sealed record CappedStatementOfWorkExceptionCreatedAtOffsetAdjuster(double Weight) : ICappedStatementOfWorkExceptionModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(CappedStatementOfWorkException item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record CapCappedStatementOfWorkExceptionAdjuster(double Cap) : ICappedStatementOfWorkExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedStatementOfWorkException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelCappedStatementOfWorkExceptionAdjuster(Dictionary<string, double> Factors) : ICappedStatementOfWorkExceptionModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CappedStatementOfWorkException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCappedStatementOfWorkExceptionAdjuster(Registry Flags, string Flag, double Percent) : ICappedStatementOfWorkExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedStatementOfWorkException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedStatementOfWorkExceptionAdjuster(List<double> Bounds, List<double> Rates) : ICappedStatementOfWorkExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedStatementOfWorkException item, double valueValue)
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

/// <summary>Runs the capped statement of work exception steps in order and keeps a trace of each.</summary>
public sealed class CappedStatementOfWorkExceptionEngine(List<ICappedStatementOfWorkExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedStatementOfWorkExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedStatementOfWorkExceptionEngine DefaultCappedStatementOfWorkExceptionEngine(Registry flags)
    {
        return new CappedStatementOfWorkExceptionEngine(
            new List<ICappedStatementOfWorkExceptionModifier>
            {
                new CappedStatementOfWorkExceptionCreatedAtOffsetAdjuster(1.95),
                new CapCappedStatementOfWorkExceptionAdjuster(25515.0),
                new ChannelCappedStatementOfWorkExceptionAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.13, ["west"] = 0.94, ["retail"] = 1.07 }),
                new FlaggedCappedStatementOfWorkExceptionAdjuster(
                    flags,
                    "capped-statement-of-work-exception-global",
                    8.8),
                new TieredCappedStatementOfWorkExceptionAdjuster(
                    new List<double> { 1352.0, 3196.0 },
                    new List<double> { 1.05, 0.97 }),
            },
            new List<string>(),
            0.8,
            54525.0);
    }

    public double Run(CappedStatementOfWorkException item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedStatementOfWorkException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
