namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional succession plan allocation must pass. Check returns its problems, or none.</summary>
public interface IRegionalSuccessionPlanAllocationRule
{
    List<string> Check(RegionalSuccessionPlanAllocation item);
}

public sealed record RegionalSuccessionPlanAllocationTeamLengthRule(int MaxLength) : IRegionalSuccessionPlanAllocationRule
{
    public List<string> Check(RegionalSuccessionPlanAllocation item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAllocationLabelLengthRule(int MaxLength) : IRegionalSuccessionPlanAllocationRule
{
    public List<string> Check(RegionalSuccessionPlanAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAllocationRequiredLabelRule(string Message) : IRegionalSuccessionPlanAllocationRule
{
    public List<string> Check(RegionalSuccessionPlanAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAllocationAllowedPhoneRule(List<string> Allowed) : IRegionalSuccessionPlanAllocationRule
{
    public List<string> Check(RegionalSuccessionPlanAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAllocationRequiredDepartmentRule(string Message) : IRegionalSuccessionPlanAllocationRule
{
    public List<string> Check(RegionalSuccessionPlanAllocation item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional succession plan allocation.</summary>
public sealed record RegionalSuccessionPlanAllocationValidator(List<IRegionalSuccessionPlanAllocationRule> Rules)
{
    public List<string> Validate(RegionalSuccessionPlanAllocation item)
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

    public void RequireValid(RegionalSuccessionPlanAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSuccessionPlanAllocation> items)
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

/// <summary>Functions of regional succession plan allocation rules.</summary>
public static class RegionalSuccessionPlanAllocationRules
{
    public static RegionalSuccessionPlanAllocationValidator DefaultRegionalSuccessionPlanAllocationValidator()
    {
        return new RegionalSuccessionPlanAllocationValidator(
            new List<IRegionalSuccessionPlanAllocationRule>
            {
                new RegionalSuccessionPlanAllocationTeamLengthRule(64),
                new RegionalSuccessionPlanAllocationLabelLengthRule(255),
                new RegionalSuccessionPlanAllocationRequiredLabelRule(
                    "regional succession plan allocation needs a value"),
                new RegionalSuccessionPlanAllocationAllowedPhoneRule(
                    new List<string> { "seasonal", "rolling", "adaptive" }),
                new RegionalSuccessionPlanAllocationRequiredDepartmentRule(
                    "regional succession plan allocation needs a value"),
            });
    }
}
