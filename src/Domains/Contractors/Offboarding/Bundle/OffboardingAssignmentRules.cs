namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding assignment must pass. Check returns its problems, or none.</summary>
public interface IOffboardingAssignmentRule
{
    List<string> Check(OffboardingAssignment item);
}

public sealed record OffboardingAssignmentPinnedGuardRule(int Limit) : IOffboardingAssignmentRule
{
    public List<string> Check(OffboardingAssignment item)
    {
        if (item.Pinned && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAssignmentAllowedBadgeRule(List<string> Allowed) : IOffboardingAssignmentRule
{
    public List<string> Check(OffboardingAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAssignmentBadgeLengthRule(int MaxLength) : IOffboardingAssignmentRule
{
    public List<string> Check(OffboardingAssignment item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding assignment.</summary>
public sealed record OffboardingAssignmentValidator(List<IOffboardingAssignmentRule> Rules)
{
    public List<string> Validate(OffboardingAssignment item)
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

    public void RequireValid(OffboardingAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingAssignment> items)
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

/// <summary>Functions of offboarding assignment rules.</summary>
public static class OffboardingAssignmentRules
{
    public static OffboardingAssignmentValidator DefaultOffboardingAssignmentValidator()
    {
        return new OffboardingAssignmentValidator(
            new List<IOffboardingAssignmentRule>
            {
                new OffboardingAssignmentPinnedGuardRule(80),
                new OffboardingAssignmentAllowedBadgeRule(new List<string>()),
                new OffboardingAssignmentBadgeLengthRule(32),
            });
    }
}
