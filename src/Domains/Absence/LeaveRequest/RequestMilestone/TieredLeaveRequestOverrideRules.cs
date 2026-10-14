namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered leave request override must pass. Check returns its problems, or none.</summary>
public interface ITieredLeaveRequestOverrideRule
{
    List<string> Check(TieredLeaveRequestOverride item);
}

public sealed record TieredLeaveRequestOverrideAllowedCategoryRule(List<string> Allowed) : ITieredLeaveRequestOverrideRule
{
    public List<string> Check(TieredLeaveRequestOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record TieredLeaveRequestOverrideRequiredTitleRule(string Message) : ITieredLeaveRequestOverrideRule
{
    public List<string> Check(TieredLeaveRequestOverride item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLeaveRequestOverrideTitleLengthRule(int MaxLength) : ITieredLeaveRequestOverrideRule
{
    public List<string> Check(TieredLeaveRequestOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered leave request override.</summary>
public sealed record TieredLeaveRequestOverrideValidator(List<ITieredLeaveRequestOverrideRule> Rules)
{
    public List<string> Validate(TieredLeaveRequestOverride item)
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

    public void RequireValid(TieredLeaveRequestOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLeaveRequestOverride> items)
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

/// <summary>Functions of tiered leave request override rules.</summary>
public static class TieredLeaveRequestOverrideRules
{
    public static TieredLeaveRequestOverrideValidator DefaultTieredLeaveRequestOverrideValidator()
    {
        return new TieredLeaveRequestOverrideValidator(
            new List<ITieredLeaveRequestOverrideRule>
            {
                new TieredLeaveRequestOverrideAllowedCategoryRule(new List<string> { "rolling", "manual", "legacy" }),
                new TieredLeaveRequestOverrideRequiredTitleRule("tiered leave request override needs a value"),
                new TieredLeaveRequestOverrideTitleLengthRule(1000),
            });
    }
}
