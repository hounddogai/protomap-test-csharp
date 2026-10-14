namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict accrual exception calculation.</summary>
public interface IStrictAccrualExceptionStep
{
    string Describe();
    double Adjust(StrictAccrualException item, double valueValue);
}

public sealed record CapStrictAccrualExceptionAdjuster(double Cap) : IStrictAccrualExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAccrualException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedStrictAccrualExceptionAdjuster(Registry Flags, string Flag, double Percent) : IStrictAccrualExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictAccrualException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelStrictAccrualExceptionAdjuster(Dictionary<string, double> Factors) : IStrictAccrualExceptionStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictAccrualException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingStrictAccrualExceptionAdjuster(double Step) : IStrictAccrualExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAccrualException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict accrual exception steps in order and keeps a trace of each.</summary>
public sealed class StrictAccrualExceptionEngine(List<IStrictAccrualExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAccrualExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAccrualExceptionEngine DefaultStrictAccrualExceptionEngine(Registry flags)
    {
        return new StrictAccrualExceptionEngine(
            new List<IStrictAccrualExceptionStep>
            {
                new CapStrictAccrualExceptionAdjuster(28523.0),
                new FlaggedStrictAccrualExceptionAdjuster(flags, "strict-accrual-exception-lenient", 3.8),
                new LabelStrictAccrualExceptionAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.95, ["central"] = 1.35, ["online"] = 1.19 }),
                new RoundingStrictAccrualExceptionAdjuster(5.0),
            },
            new List<string>(),
            3.0,
            70594.6);
    }

    public double Evaluate(StrictAccrualException item)
    {
        var current = item.WeightedLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<StrictAccrualException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
