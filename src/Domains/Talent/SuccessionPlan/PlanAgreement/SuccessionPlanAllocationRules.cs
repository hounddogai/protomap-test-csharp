namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan allocation must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanAllocationRule
{
    List<string> Check(SuccessionPlanAllocation item);
}

public sealed record SuccessionPlanAllocationCodePrefixRule(string Prefix) : ISuccessionPlanAllocationRule
{
    public List<string> Check(SuccessionPlanAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAllocationRequiredSkillRule(string Message) : ISuccessionPlanAllocationRule
{
    public List<string> Check(SuccessionPlanAllocation item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAllocationCodeLengthRule(int MaxLength) : ISuccessionPlanAllocationRule
{
    public List<string> Check(SuccessionPlanAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAllocationAllowedLabelRule(List<string> Allowed) : ISuccessionPlanAllocationRule
{
    public List<string> Check(SuccessionPlanAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan allocation.</summary>
public sealed record SuccessionPlanAllocationValidator(List<ISuccessionPlanAllocationRule> Rules)
{
    public List<string> Validate(SuccessionPlanAllocation item)
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

    public void RequireValid(SuccessionPlanAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanAllocation> items)
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

/// <summary>Functions of succession plan allocation rules.</summary>
public static class SuccessionPlanAllocationRules
{
    public static SuccessionPlanAllocationValidator DefaultSuccessionPlanAllocationValidator()
    {
        return new SuccessionPlanAllocationValidator(
            new List<ISuccessionPlanAllocationRule>
            {
                new SuccessionPlanAllocationCodePrefixRule("Q"),
                new SuccessionPlanAllocationRequiredSkillRule("succession plan allocation needs an entry"),
                new SuccessionPlanAllocationCodeLengthRule(255),
                new SuccessionPlanAllocationAllowedLabelRule(new List<string>()),
            });
    }
}
