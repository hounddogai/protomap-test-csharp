namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient offboarding profile must pass. Check returns its problems, or none.</summary>
public interface ILenientOffboardingProfileRule
{
    List<string> Check(LenientOffboardingProfile item);
}

public sealed record LenientOffboardingProfileCodePrefixRule(string Prefix) : ILenientOffboardingProfileRule
{
    public List<string> Check(LenientOffboardingProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientOffboardingProfileGradeLengthRule(int MaxLength) : ILenientOffboardingProfileRule
{
    public List<string> Check(LenientOffboardingProfile item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientOffboardingProfileAllowedGradeRule(List<string> Allowed) : ILenientOffboardingProfileRule
{
    public List<string> Check(LenientOffboardingProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record LenientOffboardingProfileCodeLengthRule(int MaxLength) : ILenientOffboardingProfileRule
{
    public List<string> Check(LenientOffboardingProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientOffboardingProfileRequiredGradeRule(string Message) : ILenientOffboardingProfileRule
{
    public List<string> Check(LenientOffboardingProfile item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient offboarding profile.</summary>
public sealed record LenientOffboardingProfileValidator(List<ILenientOffboardingProfileRule> Rules)
{
    public List<string> Validate(LenientOffboardingProfile item)
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

    public void RequireValid(LenientOffboardingProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientOffboardingProfile> items)
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

/// <summary>Functions of lenient offboarding profile rules.</summary>
public static class LenientOffboardingProfileRules
{
    public static LenientOffboardingProfileValidator DefaultLenientOffboardingProfileValidator()
    {
        return new LenientOffboardingProfileValidator(
            new List<ILenientOffboardingProfileRule>
            {
                new LenientOffboardingProfileCodePrefixRule("ACM"),
                new LenientOffboardingProfileGradeLengthRule(16),
                new LenientOffboardingProfileAllowedGradeRule(new List<string>()),
                new LenientOffboardingProfileCodeLengthRule(86400),
                new LenientOffboardingProfileRequiredGradeRule("lenient offboarding profile needs a value"),
            });
    }
}
