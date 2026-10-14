namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary timesheet group calculation.</summary>
public interface IPrimaryTimesheetGroupAdjuster
{
    string Describe();
    double Adjust(PrimaryTimesheetGroup item, double valueValue);
}

public sealed record PrimaryTimesheetGroupWeightOffsetAdjuster(double Weight) : IPrimaryTimesheetGroupAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(PrimaryTimesheetGroup item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CountryPrimaryTimesheetGroupAdjuster(Dictionary<string, double> Factors) : IPrimaryTimesheetGroupAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PrimaryTimesheetGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPrimaryTimesheetGroupAdjuster(double Minimum) : IPrimaryTimesheetGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryTimesheetGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPrimaryTimesheetGroupAdjuster(double Step) : IPrimaryTimesheetGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryTimesheetGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPrimaryTimesheetGroupAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryTimesheetGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryTimesheetGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary timesheet group steps in order and keeps a trace of each.</summary>
public sealed class PrimaryTimesheetGroupEngine(List<IPrimaryTimesheetGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryTimesheetGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryTimesheetGroupEngine DefaultPrimaryTimesheetGroupEngine(Registry flags)
    {
        return new PrimaryTimesheetGroupEngine(
            new List<IPrimaryTimesheetGroupAdjuster>
            {
                new PrimaryTimesheetGroupWeightOffsetAdjuster(0.19),
                new CountryPrimaryTimesheetGroupAdjuster(new Dictionary<string, double> { ["east"] = 0.79 }),
                new MinimumPrimaryTimesheetGroupAdjuster(10.8),
                new RoundingPrimaryTimesheetGroupAdjuster(1.0),
                new FlaggedPrimaryTimesheetGroupAdjuster(flags, "primary-timesheet-group-tiered", 6.7),
            },
            new List<string>(),
            3.4,
            29117.9);
    }

    public double Run(PrimaryTimesheetGroup item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryTimesheetGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
