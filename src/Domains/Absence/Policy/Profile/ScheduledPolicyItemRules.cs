namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled policy item must pass. Check returns its problems, or none.</summary>
public interface IScheduledPolicyItemRule
{
    List<string> Check(ScheduledPolicyItem item);
}

public sealed record ScheduledPolicyItemRequiredCodeRule(string Message) : IScheduledPolicyItemRule
{
    public List<string> Check(ScheduledPolicyItem item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPolicyItemRequiredTitleRule(string Message) : IScheduledPolicyItemRule
{
    public List<string> Check(ScheduledPolicyItem item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPolicyItemAllowedDepartmentRule(List<string> Allowed) : IScheduledPolicyItemRule
{
    public List<string> Check(ScheduledPolicyItem item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled policy item.</summary>
public sealed record ScheduledPolicyItemValidator(List<IScheduledPolicyItemRule> Rules)
{
    public List<string> Validate(ScheduledPolicyItem item)
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

    public void RequireValid(ScheduledPolicyItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPolicyItem> items)
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

/// <summary>Functions of scheduled policy item rules.</summary>
public static class ScheduledPolicyItemRules
{
    public static ScheduledPolicyItemValidator DefaultScheduledPolicyItemValidator()
    {
        return new ScheduledPolicyItemValidator(
            new List<IScheduledPolicyItemRule>
            {
                new ScheduledPolicyItemRequiredCodeRule("scheduled policy item needs an entry"),
                new ScheduledPolicyItemRequiredTitleRule("scheduled policy item needs a value"),
                new ScheduledPolicyItemAllowedDepartmentRule(new List<string>()),
            });
    }
}
