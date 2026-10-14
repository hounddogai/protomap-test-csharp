namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling occupancy allocation must pass. Check returns its problems, or none.</summary>
public interface IRollingOccupancyAllocationRule
{
    List<string> Check(RollingOccupancyAllocation item);
}

public sealed record RollingOccupancyAllocationAllowedDepartmentRule(List<string> Allowed) : IRollingOccupancyAllocationRule
{
    public List<string> Check(RollingOccupancyAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RollingOccupancyAllocationRequiredNameRule(string Message) : IRollingOccupancyAllocationRule
{
    public List<string> Check(RollingOccupancyAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingOccupancyAllocationDepartmentLengthRule(int MaxLength) : IRollingOccupancyAllocationRule
{
    public List<string> Check(RollingOccupancyAllocation item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingOccupancyAllocationCodePrefixRule(string Prefix) : IRollingOccupancyAllocationRule
{
    public List<string> Check(RollingOccupancyAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingOccupancyAllocationBadgeLengthRule(int MaxLength) : IRollingOccupancyAllocationRule
{
    public List<string> Check(RollingOccupancyAllocation item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling occupancy allocation.</summary>
public sealed record RollingOccupancyAllocationValidator(List<IRollingOccupancyAllocationRule> Rules)
{
    public List<string> Validate(RollingOccupancyAllocation item)
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

    public void RequireValid(RollingOccupancyAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingOccupancyAllocation> items)
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

/// <summary>Functions of rolling occupancy allocation rules.</summary>
public static class RollingOccupancyAllocationRules
{
    public static RollingOccupancyAllocationValidator DefaultRollingOccupancyAllocationValidator()
    {
        return new RollingOccupancyAllocationValidator(
            new List<IRollingOccupancyAllocationRule>
            {
                new RollingOccupancyAllocationAllowedDepartmentRule(
                    new List<string> { "global", "default", "tiered" }),
                new RollingOccupancyAllocationRequiredNameRule("rolling occupancy allocation needs a value"),
                new RollingOccupancyAllocationDepartmentLengthRule(86400),
                new RollingOccupancyAllocationCodePrefixRule("Q"),
                new RollingOccupancyAllocationBadgeLengthRule(64),
            });
    }
}
