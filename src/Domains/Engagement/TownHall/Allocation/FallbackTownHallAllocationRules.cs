namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback town hall allocation must pass. Check returns its problems, or none.</summary>
public interface IFallbackTownHallAllocationRule
{
    List<string> Check(FallbackTownHallAllocation item);
}

public sealed record FallbackTownHallAllocationAllowedEmployeeIdRule(List<string> Allowed) : IFallbackTownHallAllocationRule
{
    public List<string> Check(FallbackTownHallAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is stale" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTownHallAllocationAttemptsRangeRule(int Min, int Max) : IFallbackTownHallAllocationRule
{
    public List<string> Check(FallbackTownHallAllocation item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackTownHallAllocationRequiredTitleRule(string Message) : IFallbackTownHallAllocationRule
{
    public List<string> Check(FallbackTownHallAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback town hall allocation.</summary>
public sealed record FallbackTownHallAllocationValidator(List<IFallbackTownHallAllocationRule> Rules)
{
    public List<string> Validate(FallbackTownHallAllocation item)
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

    public void RequireValid(FallbackTownHallAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackTownHallAllocation> items)
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

/// <summary>Functions of fallback town hall allocation rules.</summary>
public static class FallbackTownHallAllocationRules
{
    public static FallbackTownHallAllocationValidator DefaultFallbackTownHallAllocationValidator()
    {
        return new FallbackTownHallAllocationValidator(
            new List<IFallbackTownHallAllocationRule>
            {
                new FallbackTownHallAllocationAllowedEmployeeIdRule(
                    new List<string> { "global", "weighted", "primary" }),
                new FallbackTownHallAllocationAttemptsRangeRule(80, 3600),
                new FallbackTownHallAllocationRequiredTitleRule("fallback town hall allocation needs an entry"),
            });
    }
}
