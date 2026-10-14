namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan override must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanOverrideRule
{
    List<string> Check(SuccessionPlanOverride item);
}

public sealed record SuccessionPlanOverrideNameLengthRule(int MaxLength) : ISuccessionPlanOverrideRule
{
    public List<string> Check(SuccessionPlanOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanOverrideAllowedLocaleRule(List<string> Allowed) : ISuccessionPlanOverrideRule
{
    public List<string> Check(SuccessionPlanOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanOverrideRequiredNameRule(string Message) : ISuccessionPlanOverrideRule
{
    public List<string> Check(SuccessionPlanOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanOverrideCreatedAtRangeRule(int Min, int Max) : ISuccessionPlanOverrideRule
{
    public List<string> Check(SuccessionPlanOverride item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan override.</summary>
public sealed record SuccessionPlanOverrideValidator(List<ISuccessionPlanOverrideRule> Rules)
{
    public List<string> Validate(SuccessionPlanOverride item)
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

    public void RequireValid(SuccessionPlanOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanOverride> items)
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

/// <summary>Functions of succession plan override rules.</summary>
public static class SuccessionPlanOverrideRules
{
    public static SuccessionPlanOverrideValidator DefaultSuccessionPlanOverrideValidator()
    {
        return new SuccessionPlanOverrideValidator(
            new List<ISuccessionPlanOverrideRule>
            {
                new SuccessionPlanOverrideNameLengthRule(32),
                new SuccessionPlanOverrideAllowedLocaleRule(new List<string>()),
                new SuccessionPlanOverrideRequiredNameRule("succession plan override needs this field"),
                new SuccessionPlanOverrideCreatedAtRangeRule(16, 120),
            });
    }
}
