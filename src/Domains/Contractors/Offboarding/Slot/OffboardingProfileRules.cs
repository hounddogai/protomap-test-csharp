namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding profile must pass. Check returns its problems, or none.</summary>
public interface IOffboardingProfileRule
{
    List<string> Check(OffboardingProfile item);
}

public sealed record OffboardingProfileRequiredCodeRule(string Message) : IOffboardingProfileRule
{
    public List<string> Check(OffboardingProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingProfileRequiredManagerIdRule(string Message) : IOffboardingProfileRule
{
    public List<string> Check(OffboardingProfile item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingProfileAllowedLabelRule(List<string> Allowed) : IOffboardingProfileRule
{
    public List<string> Check(OffboardingProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingProfileCreatedAtRangeRule(int Min, int Max) : IOffboardingProfileRule
{
    public List<string> Check(OffboardingProfile item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding profile.</summary>
public sealed record OffboardingProfileValidator(List<IOffboardingProfileRule> Rules)
{
    public List<string> Validate(OffboardingProfile item)
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

    public void RequireValid(OffboardingProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingProfile> items)
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

/// <summary>Functions of offboarding profile rules.</summary>
public static class OffboardingProfileRules
{
    public static OffboardingProfileValidator DefaultOffboardingProfileValidator()
    {
        return new OffboardingProfileValidator(
            new List<IOffboardingProfileRule>
            {
                new OffboardingProfileRequiredCodeRule("offboarding profile needs a value"),
                new OffboardingProfileRequiredManagerIdRule("offboarding profile needs a value"),
                new OffboardingProfileAllowedLabelRule(new List<string> { "fallback", "weighted" }),
                new OffboardingProfileCreatedAtRangeRule(255, 255),
            });
    }
}
