namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy schedule must pass. Check returns its problems, or none.</summary>
public interface IOccupancyScheduleRule
{
    List<string> Check(OccupancySchedule item);
}

public sealed record OccupancyScheduleTitleLengthRule(int MaxLength) : IOccupancyScheduleRule
{
    public List<string> Check(OccupancySchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyScheduleAllowedCurrencyRule(List<string> Allowed) : IOccupancyScheduleRule
{
    public List<string> Check(OccupancySchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyScheduleRequiredTitleRule(string Message) : IOccupancyScheduleRule
{
    public List<string> Check(OccupancySchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy schedule.</summary>
public sealed record OccupancyScheduleValidator(List<IOccupancyScheduleRule> Rules)
{
    public List<string> Validate(OccupancySchedule item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(OccupancySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancySchedule> items)
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

/// <summary>Functions of occupancy schedule rules.</summary>
public static class OccupancyScheduleRules
{
    public static OccupancyScheduleValidator DefaultOccupancyScheduleValidator()
    {
        return new OccupancyScheduleValidator(
            new List<IOccupancyScheduleRule>
            {
                new OccupancyScheduleTitleLengthRule(255),
                new OccupancyScheduleAllowedCurrencyRule(new List<string> { "weighted", "pending", "strict" }),
                new OccupancyScheduleRequiredTitleRule("occupancy schedule needs an entry"),
            });
    }
}
