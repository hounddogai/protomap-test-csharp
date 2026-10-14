namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual leave request plan calculation.</summary>
public interface IManualLeaveRequestPlanModifier
{
    string Describe();
    double Adjust(ManualLeaveRequestPlan item, double valueValue);
}

public sealed record RoundingManualLeaveRequestPlanAdjuster(double Step) : IManualLeaveRequestPlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualLeaveRequestPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ManualLeaveRequestPlanWindowDaysOffsetAdjuster(double Weight) : IManualLeaveRequestPlanModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(ManualLeaveRequestPlan item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record LocaleManualLeaveRequestPlanAdjuster(Dictionary<string, double> Factors) : IManualLeaveRequestPlanModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ManualLeaveRequestPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualLeaveRequestPlanAdjuster(List<double> Bounds, List<double> Rates) : IManualLeaveRequestPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualLeaveRequestPlan item, double valueValue)
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

/// <summary>Runs the manual leave request plan steps in order and keeps a trace of each.</summary>
public sealed class ManualLeaveRequestPlanEngine(List<IManualLeaveRequestPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualLeaveRequestPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualLeaveRequestPlanEngine DefaultManualLeaveRequestPlanEngine()
    {
        return new ManualLeaveRequestPlanEngine(
            new List<IManualLeaveRequestPlanModifier>
            {
                new RoundingManualLeaveRequestPlanAdjuster(0.1),
                new ManualLeaveRequestPlanWindowDaysOffsetAdjuster(1.93),
                new LocaleManualLeaveRequestPlanAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.99, ["north"] = 0.9, ["south"] = 0.81 }),
                new TieredManualLeaveRequestPlanAdjuster(
                    new List<double> { 4068.0, 7087.0 },
                    new List<double> { 0.96, 1.19 }),
            },
            new List<string>(),
            2.0,
            16341.8);
    }

    public double Run(ManualLeaveRequestPlan item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualLeaveRequestPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
