namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding plan must pass. Check returns its problems, or none.</summary>
public interface IOffboardingPlanRule
{
    List<string> Check(OffboardingPlan item);
}

public sealed record OffboardingPlanDescriptionLengthRule(int MaxLength) : IOffboardingPlanRule
{
    public List<string> Check(OffboardingPlan item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingPlanAllowedCodeRule(List<string> Allowed) : IOffboardingPlanRule
{
    public List<string> Check(OffboardingPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingPlanActiveGuardRule(int Limit) : IOffboardingPlanRule
{
    public List<string> Check(OffboardingPlan item)
    {
        if (item.Active && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding plan.</summary>
public sealed record OffboardingPlanValidator(List<IOffboardingPlanRule> Rules)
{
    public List<string> Validate(OffboardingPlan item)
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

    public void RequireValid(OffboardingPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingPlan> items)
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

/// <summary>Functions of offboarding plan rules.</summary>
public static class OffboardingPlanRules
{
    public static OffboardingPlanValidator DefaultOffboardingPlanValidator()
    {
        return new OffboardingPlanValidator(
            new List<IOffboardingPlanRule>
            {
                new OffboardingPlanDescriptionLengthRule(80),
                new OffboardingPlanAllowedCodeRule(new List<string> { "manual", "global", "capped" }),
                new OffboardingPlanActiveGuardRule(80),
            });
    }
}
