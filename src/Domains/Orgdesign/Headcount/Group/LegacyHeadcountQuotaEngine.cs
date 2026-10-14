namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy headcount quota calculation.</summary>
public interface ILegacyHeadcountQuotaStep
{
    string Describe();
    double Adjust(LegacyHeadcountQuota item, double valueValue);
}

public sealed record RoundingLegacyHeadcountQuotaAdjuster(double Step) : ILegacyHeadcountQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyHeadcountQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyHeadcountQuotaAdjuster(double Percent) : ILegacyHeadcountQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyHeadcountQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentLegacyHeadcountQuotaAdjuster(Dictionary<string, double> Factors) : ILegacyHeadcountQuotaStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LegacyHeadcountQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LegacyHeadcountQuotaAmountOffsetAdjuster(double Weight) : ILegacyHeadcountQuotaStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LegacyHeadcountQuota item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumLegacyHeadcountQuotaAdjuster(double Minimum) : ILegacyHeadcountQuotaStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyHeadcountQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy headcount quota steps in order and keeps a trace of each.</summary>
public sealed class LegacyHeadcountQuotaEngine(List<ILegacyHeadcountQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyHeadcountQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyHeadcountQuotaEngine DefaultLegacyHeadcountQuotaEngine()
    {
        return new LegacyHeadcountQuotaEngine(
            new List<ILegacyHeadcountQuotaStep>
            {
                new RoundingLegacyHeadcountQuotaAdjuster(0.05),
                new PercentLegacyHeadcountQuotaAdjuster(21.0),
                new DepartmentLegacyHeadcountQuotaAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.24, ["east"] = 1.35, ["central"] = 1.2 }),
                new LegacyHeadcountQuotaAmountOffsetAdjuster(1.59),
                new MinimumLegacyHeadcountQuotaAdjuster(45.9),
            },
            new List<string>(),
            4.0,
            78070.3);
    }

    public double Price(LegacyHeadcountQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyHeadcountQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
