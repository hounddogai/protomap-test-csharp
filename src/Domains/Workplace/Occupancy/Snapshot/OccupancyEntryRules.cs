namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy entry must pass. Check returns its problems, or none.</summary>
public interface IOccupancyEntryRule
{
    List<string> Check(OccupancyEntry item);
}

public sealed record OccupancyEntryWindowDaysRangeRule(int Min, int Max) : IOccupancyEntryRule
{
    public List<string> Check(OccupancyEntry item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyEntryRequiredCodeRule(string Message) : IOccupancyEntryRule
{
    public List<string> Check(OccupancyEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyEntryAllowedJobCodeRule(List<string> Allowed) : IOccupancyEntryRule
{
    public List<string> Check(OccupancyEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyEntryRequiredJobCodeRule(string Message) : IOccupancyEntryRule
{
    public List<string> Check(OccupancyEntry item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyEntrySourceLengthRule(int MaxLength) : IOccupancyEntryRule
{
    public List<string> Check(OccupancyEntry item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyEntryFteRangeRule(double Min, double Max) : IOccupancyEntryRule
{
    public List<string> Check(OccupancyEntry item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a occupancy entry.</summary>
public sealed record OccupancyEntryValidator(List<IOccupancyEntryRule> Rules)
{
    public List<string> Validate(OccupancyEntry item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(OccupancyEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyEntry> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of occupancy entry rules.</summary>
public static class OccupancyEntryRules
{
    public static OccupancyEntryValidator DefaultOccupancyEntryValidator()
    {
        return new OccupancyEntryValidator(
            new List<IOccupancyEntryRule>
            {
                new OccupancyEntryWindowDaysRangeRule(255, 3600),
                new OccupancyEntryRequiredCodeRule("occupancy entry needs a value"),
                new OccupancyEntryAllowedJobCodeRule(new List<string> { "seasonal", "scheduled", "adaptive" }),
                new OccupancyEntryRequiredJobCodeRule("occupancy entry needs a value"),
                new OccupancyEntrySourceLengthRule(255),
                new OccupancyEntryFteRangeRule(4.5, 2691.6),
            });
    }
}
