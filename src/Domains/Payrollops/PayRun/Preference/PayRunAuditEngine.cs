namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run audit calculation.</summary>
public interface IPayRunAuditAdjuster
{
    string Describe();
    double Adjust(PayRunAudit item, double valueValue);
}

public sealed record PercentPayRunAuditAdjuster(double Percent) : IPayRunAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPayRunAuditAdjuster(double Step) : IPayRunAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayRunAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPayRunAuditAdjuster(double Cap) : IPayRunAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitlePayRunAuditAdjuster(Dictionary<string, double> Factors) : IPayRunAuditAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayRunAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPayRunAuditAdjuster(double Minimum) : IPayRunAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PayRunAuditStartsAtOffsetAdjuster(double Weight) : IPayRunAuditAdjuster
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(PayRunAudit item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

/// <summary>Runs the pay run audit steps in order and keeps a trace of each.</summary>
public sealed class PayRunAuditEngine(List<IPayRunAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunAuditEngine DefaultPayRunAuditEngine()
    {
        return new PayRunAuditEngine(
            new List<IPayRunAuditAdjuster>
            {
                new PercentPayRunAuditAdjuster(22.4),
                new RoundingPayRunAuditAdjuster(0.05),
                new CapPayRunAuditAdjuster(12019.0),
                new TitlePayRunAuditAdjuster(new Dictionary<string, double> { ["online"] = 1.32, ["retail"] = 1.07 }),
                new MinimumPayRunAuditAdjuster(23.6),
                new PayRunAuditStartsAtOffsetAdjuster(0.53),
            },
            new List<string>(),
            0.7,
            43194.7);
    }

    public double Evaluate(PayRunAudit item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
