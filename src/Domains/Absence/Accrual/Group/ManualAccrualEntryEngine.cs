namespace Acme.Workforce.Absence.Accrual.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual accrual entry calculation.</summary>
public interface IManualAccrualEntryFactor
{
    string Describe();
    double Adjust(ManualAccrualEntry item, double valueValue);
}

public sealed record CapManualAccrualEntryAdjuster(double Cap) : IManualAccrualEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualAccrualEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingManualAccrualEntryAdjuster(double Step) : IManualAccrualEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAccrualEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumManualAccrualEntryAdjuster(double Minimum) : IManualAccrualEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualAccrualEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelManualAccrualEntryAdjuster(Dictionary<string, double> Factors) : IManualAccrualEntryFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ManualAccrualEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the manual accrual entry steps in order and keeps a trace of each.</summary>
public sealed class ManualAccrualEntryEngine(List<IManualAccrualEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAccrualEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAccrualEntryEngine DefaultManualAccrualEntryEngine()
    {
        return new ManualAccrualEntryEngine(
            new List<IManualAccrualEntryFactor>
            {
                new CapManualAccrualEntryAdjuster(40554.0),
                new RoundingManualAccrualEntryAdjuster(0.25),
                new MinimumManualAccrualEntryAdjuster(5.8),
                new LabelManualAccrualEntryAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.85, ["south"] = 1.3, ["central"] = 1.07 }),
            },
            new List<string>(),
            3.5,
            2602.3);
    }

    public double Evaluate(ManualAccrualEntry item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualAccrualEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
