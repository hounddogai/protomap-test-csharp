namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy group must pass. Check returns its problems, or none.</summary>
public interface IOccupancyGroupRule
{
    List<string> Check(OccupancyGroup item);
}

public sealed record OccupancyGroupStartsAtRangeRule(int Min, int Max) : IOccupancyGroupRule
{
    public List<string> Check(OccupancyGroup item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyGroupAllowedCodeRule(List<string> Allowed) : IOccupancyGroupRule
{
    public List<string> Check(OccupancyGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyGroupCostCenterLengthRule(int MaxLength) : IOccupancyGroupRule
{
    public List<string> Check(OccupancyGroup item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy group.</summary>
public sealed record OccupancyGroupValidator(List<IOccupancyGroupRule> Rules)
{
    public List<string> Validate(OccupancyGroup item)
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

    public void RequireValid(OccupancyGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyGroup> items)
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

/// <summary>Functions of occupancy group rules.</summary>
public static class OccupancyGroupRules
{
    public static OccupancyGroupValidator DefaultOccupancyGroupValidator()
    {
        return new OccupancyGroupValidator(
            new List<IOccupancyGroupRule>
            {
                new OccupancyGroupStartsAtRangeRule(120, 3600),
                new OccupancyGroupAllowedCodeRule(new List<string> { "tiered" }),
                new OccupancyGroupCostCenterLengthRule(120),
            });
    }
}
