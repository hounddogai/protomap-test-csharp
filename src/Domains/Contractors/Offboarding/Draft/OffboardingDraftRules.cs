namespace Acme.Workforce.Contractors.Offboarding.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding draft must pass. Check returns its problems, or none.</summary>
public interface IOffboardingDraftRule
{
    List<string> Check(OffboardingDraft item);
}

public sealed record OffboardingDraftTitleLengthRule(int MaxLength) : IOffboardingDraftRule
{
    public List<string> Check(OffboardingDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingDraftRequiredTitleRule(string Message) : IOffboardingDraftRule
{
    public List<string> Check(OffboardingDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingDraftNameLengthRule(int MaxLength) : IOffboardingDraftRule
{
    public List<string> Check(OffboardingDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingDraftAllowedCodeRule(List<string> Allowed) : IOffboardingDraftRule
{
    public List<string> Check(OffboardingDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding draft.</summary>
public sealed record OffboardingDraftValidator(List<IOffboardingDraftRule> Rules)
{
    public List<string> Validate(OffboardingDraft item)
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

    public void RequireValid(OffboardingDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingDraft> items)
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

/// <summary>Functions of offboarding draft rules.</summary>
public static class OffboardingDraftRules
{
    public static OffboardingDraftValidator DefaultOffboardingDraftValidator()
    {
        return new OffboardingDraftValidator(
            new List<IOffboardingDraftRule>
            {
                new OffboardingDraftTitleLengthRule(64),
                new OffboardingDraftRequiredTitleRule("offboarding draft needs this field"),
                new OffboardingDraftNameLengthRule(86400),
                new OffboardingDraftAllowedCodeRule(new List<string> { "pending", "weighted", "scheduled" }),
            });
    }
}
