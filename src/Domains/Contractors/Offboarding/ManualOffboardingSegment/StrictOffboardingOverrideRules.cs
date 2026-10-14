namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict offboarding override must pass. Check returns its problems, or none.</summary>
public interface IStrictOffboardingOverrideRule
{
    List<string> Check(StrictOffboardingOverride item);
}

public sealed record StrictOffboardingOverrideVisibleGuardRule(int Limit) : IStrictOffboardingOverrideRule
{
    public List<string> Check(StrictOffboardingOverride item)
    {
        if (item.Visible && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingOverrideRequiredTitleRule(string Message) : IStrictOffboardingOverrideRule
{
    public List<string> Check(StrictOffboardingOverride item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingOverrideGradeLengthRule(int MaxLength) : IStrictOffboardingOverrideRule
{
    public List<string> Check(StrictOffboardingOverride item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingOverrideTitleLengthRule(int MaxLength) : IStrictOffboardingOverrideRule
{
    public List<string> Check(StrictOffboardingOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOffboardingOverrideAllowedTitleRule(List<string> Allowed) : IStrictOffboardingOverrideRule
{
    public List<string> Check(StrictOffboardingOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict offboarding override.</summary>
public sealed record StrictOffboardingOverrideValidator(List<IStrictOffboardingOverrideRule> Rules)
{
    public List<string> Validate(StrictOffboardingOverride item)
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

    public void RequireValid(StrictOffboardingOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictOffboardingOverride> items)
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

/// <summary>Functions of strict offboarding override rules.</summary>
public static class StrictOffboardingOverrideRules
{
    public static StrictOffboardingOverrideValidator DefaultStrictOffboardingOverrideValidator()
    {
        return new StrictOffboardingOverrideValidator(
            new List<IStrictOffboardingOverrideRule>
            {
                new StrictOffboardingOverrideVisibleGuardRule(255),
                new StrictOffboardingOverrideRequiredTitleRule("strict offboarding override needs an entry"),
                new StrictOffboardingOverrideGradeLengthRule(120),
                new StrictOffboardingOverrideTitleLengthRule(86400),
                new StrictOffboardingOverrideAllowedTitleRule(new List<string>()),
            });
    }
}
