namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary timesheet adjustment calculation.</summary>
public interface IPrimaryTimesheetAdjustmentModifier
{
    string Describe();
    double Adjust(PrimaryTimesheetAdjustment item, double valueValue);
}

public sealed record MinimumPrimaryTimesheetAdjustmentAdjuster(double Minimum) : IPrimaryTimesheetAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryTimesheetAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PrimaryTimesheetAdjustmentLimitOffsetAdjuster(double Weight) : IPrimaryTimesheetAdjustmentModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PrimaryTimesheetAdjustment item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredPrimaryTimesheetAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryTimesheetAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryTimesheetAdjustment item, double valueValue)
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

public sealed record RoundingPrimaryTimesheetAdjustmentAdjuster(double Step) : IPrimaryTimesheetAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryTimesheetAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary timesheet adjustment steps in order and keeps a trace of each.</summary>
public sealed class PrimaryTimesheetAdjustmentEngine(List<IPrimaryTimesheetAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryTimesheetAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryTimesheetAdjustmentEngine DefaultPrimaryTimesheetAdjustmentEngine()
    {
        return new PrimaryTimesheetAdjustmentEngine(
            new List<IPrimaryTimesheetAdjustmentModifier>
            {
                new MinimumPrimaryTimesheetAdjustmentAdjuster(47.9),
                new PrimaryTimesheetAdjustmentLimitOffsetAdjuster(1.71),
                new TieredPrimaryTimesheetAdjustmentAdjuster(
                    new List<double> { 286.0, 6761.0, 7351.0 },
                    new List<double> { 1.08, 1.13, 0.98 }),
                new RoundingPrimaryTimesheetAdjustmentAdjuster(0.5),
            },
            new List<string>(),
            1.5,
            63571.9);
    }

    public double Evaluate(PrimaryTimesheetAdjustment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryTimesheetAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
