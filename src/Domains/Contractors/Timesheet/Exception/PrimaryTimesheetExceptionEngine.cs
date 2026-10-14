namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary timesheet exception calculation.</summary>
public interface IPrimaryTimesheetExceptionModifier
{
    string Describe();
    double Adjust(PrimaryTimesheetException item, double valueValue);
}

public sealed record CapPrimaryTimesheetExceptionAdjuster(double Cap) : IPrimaryTimesheetExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryTimesheetException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourcePrimaryTimesheetExceptionAdjuster(Dictionary<string, double> Factors) : IPrimaryTimesheetExceptionModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PrimaryTimesheetException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPrimaryTimesheetExceptionAdjuster(double Percent) : IPrimaryTimesheetExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryTimesheetException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPrimaryTimesheetExceptionAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryTimesheetExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryTimesheetException item, double valueValue)
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

public sealed record RoundingPrimaryTimesheetExceptionAdjuster(double Step) : IPrimaryTimesheetExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryTimesheetException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPrimaryTimesheetExceptionAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryTimesheetExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryTimesheetException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary timesheet exception steps in order and keeps a trace of each.</summary>
public sealed class PrimaryTimesheetExceptionEngine(List<IPrimaryTimesheetExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryTimesheetExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryTimesheetExceptionEngine DefaultPrimaryTimesheetExceptionEngine(Registry flags)
    {
        return new PrimaryTimesheetExceptionEngine(
            new List<IPrimaryTimesheetExceptionModifier>
            {
                new CapPrimaryTimesheetExceptionAdjuster(40738.0),
                new SourcePrimaryTimesheetExceptionAdjuster(new Dictionary<string, double> { ["west"] = 1.38 }),
                new PercentPrimaryTimesheetExceptionAdjuster(20.1),
                new TieredPrimaryTimesheetExceptionAdjuster(
                    new List<double> { 1378.0, 1427.0, 7934.0 },
                    new List<double> { 1.12, 0.86, 1.06 }),
                new RoundingPrimaryTimesheetExceptionAdjuster(1.0),
                new FlaggedPrimaryTimesheetExceptionAdjuster(flags, "primary-timesheet-exception-tiered", 8.2),
            },
            new List<string>(),
            2.1,
            64332.1);
    }

    public double Evaluate(PrimaryTimesheetException item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryTimesheetException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
