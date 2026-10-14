namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding override must pass. Check returns its problems, or none.</summary>
public interface IOffboardingOverrideRule
{
    List<string> Check(OffboardingOverride item);
}

public sealed record OffboardingOverrideCodeLengthRule(int MaxLength) : IOffboardingOverrideRule
{
    public List<string> Check(OffboardingOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingOverrideManagerIdLengthRule(int MaxLength) : IOffboardingOverrideRule
{
    public List<string> Check(OffboardingOverride item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingOverrideLabelLengthRule(int MaxLength) : IOffboardingOverrideRule
{
    public List<string> Check(OffboardingOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingOverrideRequiredManagerIdRule(string Message) : IOffboardingOverrideRule
{
    public List<string> Check(OffboardingOverride item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingOverrideAllowedManagerIdRule(List<string> Allowed) : IOffboardingOverrideRule
{
    public List<string> Check(OffboardingOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding override.</summary>
public sealed record OffboardingOverrideValidator(List<IOffboardingOverrideRule> Rules)
{
    public List<string> Validate(OffboardingOverride item)
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

    public void RequireValid(OffboardingOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingOverride> items)
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

/// <summary>Functions of offboarding override rules.</summary>
public static class OffboardingOverrideRules
{
    public static OffboardingOverrideValidator DefaultOffboardingOverrideValidator()
    {
        return new OffboardingOverrideValidator(
            new List<IOffboardingOverrideRule>
            {
                new OffboardingOverrideCodeLengthRule(16),
                new OffboardingOverrideManagerIdLengthRule(64),
                new OffboardingOverrideLabelLengthRule(80),
                new OffboardingOverrideRequiredManagerIdRule("offboarding override needs a value"),
                new OffboardingOverrideAllowedManagerIdRule(new List<string> { "weighted" }),
            });
    }
}
