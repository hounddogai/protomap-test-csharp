namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding entry must pass. Check returns its problems, or none.</summary>
public interface IOffboardingEntryRule
{
    List<string> Check(OffboardingEntry item);
}

public sealed record OffboardingEntryNameLengthRule(int MaxLength) : IOffboardingEntryRule
{
    public List<string> Check(OffboardingEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingEntryRequiredCodeRule(string Message) : IOffboardingEntryRule
{
    public List<string> Check(OffboardingEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingEntryAllowedNameRule(List<string> Allowed) : IOffboardingEntryRule
{
    public List<string> Check(OffboardingEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding entry.</summary>
public sealed record OffboardingEntryValidator(List<IOffboardingEntryRule> Rules)
{
    public List<string> Validate(OffboardingEntry item)
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

    public void RequireValid(OffboardingEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingEntry> items)
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

/// <summary>Functions of offboarding entry rules.</summary>
public static class OffboardingEntryRules
{
    public static OffboardingEntryValidator DefaultOffboardingEntryValidator()
    {
        return new OffboardingEntryValidator(
            new List<IOffboardingEntryRule>
            {
                new OffboardingEntryNameLengthRule(80),
                new OffboardingEntryRequiredCodeRule("offboarding entry needs an entry"),
                new OffboardingEntryAllowedNameRule(new List<string> { "capped" }),
            });
    }
}
