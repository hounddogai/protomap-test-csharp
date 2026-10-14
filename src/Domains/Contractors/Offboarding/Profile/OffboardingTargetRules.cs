namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding target must pass. Check returns its problems, or none.</summary>
public interface IOffboardingTargetRule
{
    List<string> Check(OffboardingTarget item);
}

public sealed record OffboardingTargetRequiredNameRule(string Message) : IOffboardingTargetRule
{
    public List<string> Check(OffboardingTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingTargetAllowedTitleRule(List<string> Allowed) : IOffboardingTargetRule
{
    public List<string> Check(OffboardingTarget item)
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

public sealed record OffboardingTargetUpdatedAtRangeRule(int Min, int Max) : IOffboardingTargetRule
{
    public List<string> Check(OffboardingTarget item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingTargetArchivedGuardRule(int Limit) : IOffboardingTargetRule
{
    public List<string> Check(OffboardingTarget item)
    {
        if (item.Archived && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingTargetNameLengthRule(int MaxLength) : IOffboardingTargetRule
{
    public List<string> Check(OffboardingTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding target.</summary>
public sealed record OffboardingTargetValidator(List<IOffboardingTargetRule> Rules)
{
    public List<string> Validate(OffboardingTarget item)
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

    public void RequireValid(OffboardingTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingTarget> items)
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

/// <summary>Functions of offboarding target rules.</summary>
public static class OffboardingTargetRules
{
    public static OffboardingTargetValidator DefaultOffboardingTargetValidator()
    {
        return new OffboardingTargetValidator(
            new List<IOffboardingTargetRule>
            {
                new OffboardingTargetRequiredNameRule("offboarding target needs a value"),
                new OffboardingTargetAllowedTitleRule(new List<string>()),
                new OffboardingTargetUpdatedAtRangeRule(16, 64),
                new OffboardingTargetArchivedGuardRule(3600),
                new OffboardingTargetNameLengthRule(3600),
            });
    }
}
