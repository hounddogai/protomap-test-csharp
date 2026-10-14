namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception ledger calculation.</summary>
public interface IExceptionLedgerStep
{
    string Describe();
    double Adjust(ExceptionLedger item, double valueValue);
}

public sealed record CapExceptionLedgerAdjuster(double Cap) : IExceptionLedgerStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingExceptionLedgerAdjuster(double Step) : IExceptionLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelExceptionLedgerAdjuster(Dictionary<string, double> Factors) : IExceptionLedgerStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ExceptionLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the exception ledger steps in order and keeps a trace of each.</summary>
public sealed class ExceptionLedgerEngine(List<IExceptionLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionLedgerEngine DefaultExceptionLedgerEngine()
    {
        return new ExceptionLedgerEngine(
            new List<IExceptionLedgerStep>
            {
                new CapExceptionLedgerAdjuster(26516.0),
                new RoundingExceptionLedgerAdjuster(1.0),
                new LabelExceptionLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.86, ["east"] = 0.72, ["west"] = 1.39 }),
            },
            new List<string>(),
            4.0,
            43093.2);
    }

    public double Run(ExceptionLedger item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
