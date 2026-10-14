namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary timesheet plan calculation.</summary>
public interface IPrimaryTimesheetPlanModifier
{
    string Describe();
    double Adjust(PrimaryTimesheetPlan item, double valueValue);
}

public sealed record ChannelPrimaryTimesheetPlanAdjuster(Dictionary<string, double> Factors) : IPrimaryTimesheetPlanModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PrimaryTimesheetPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryTimesheetPlanAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryTimesheetPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryTimesheetPlan item, double valueValue)
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

public sealed record CapPrimaryTimesheetPlanAdjuster(double Cap) : IPrimaryTimesheetPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryTimesheetPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary timesheet plan steps in order and keeps a trace of each.</summary>
public sealed class PrimaryTimesheetPlanEngine(List<IPrimaryTimesheetPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryTimesheetPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryTimesheetPlanEngine DefaultPrimaryTimesheetPlanEngine()
    {
        return new PrimaryTimesheetPlanEngine(
            new List<IPrimaryTimesheetPlanModifier>
            {
                new ChannelPrimaryTimesheetPlanAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.15, ["north"] = 0.99, ["online"] = 0.79 }),
                new TieredPrimaryTimesheetPlanAdjuster(
                    new List<double> { 4444.0, 8702.0, 9035.0 },
                    new List<double> { 1.16, 0.89, 1.02 }),
                new CapPrimaryTimesheetPlanAdjuster(41134.0),
            },
            new List<string>(),
            3.4,
            79969.9);
    }

    public double Compute(PrimaryTimesheetPlan item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryTimesheetPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
