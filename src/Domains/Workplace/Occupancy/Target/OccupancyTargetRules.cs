namespace Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>A rule a occupancy target must pass. Check returns its problems, or none.</summary>
public interface IOccupancyTargetRule
{
    List<string> Check(OccupancyTarget item);
}

public sealed record OccupancyTargetContactEmailLengthRule(int MaxLength) : IOccupancyTargetRule
{
    public List<string> Check(OccupancyTarget item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyTargetHoursRangeRule(double Min, double Max) : IOccupancyTargetRule
{
    public List<string> Check(OccupancyTarget item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyTargetRequiredJobCodeRule(string Message) : IOccupancyTargetRule
{
    public List<string> Check(OccupancyTarget item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy target.</summary>
public sealed record OccupancyTargetValidator(List<IOccupancyTargetRule> Rules)
{
    public List<string> Validate(OccupancyTarget item)
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

    public void RequireValid(OccupancyTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyTarget> items)
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

/// <summary>Functions of occupancy target rules.</summary>
public static class OccupancyTargetRules
{
    public static OccupancyTargetValidator DefaultOccupancyTargetValidator()
    {
        return new OccupancyTargetValidator(
            new List<IOccupancyTargetRule>
            {
                new OccupancyTargetContactEmailLengthRule(3600),
                new OccupancyTargetHoursRangeRule(9.1, 2979.5),
                new OccupancyTargetRequiredJobCodeRule("occupancy target needs this field"),
            });
    }
}
