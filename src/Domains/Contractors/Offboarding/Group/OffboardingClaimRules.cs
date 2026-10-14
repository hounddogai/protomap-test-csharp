namespace Acme.Workforce.Contractors.Offboarding.Group;

/// <summary>A rule a offboarding claim must pass. Check returns its problems, or none.</summary>
public interface IOffboardingClaimRule
{
    List<string> Check(OffboardingClaim item);
}

public sealed record OffboardingClaimExternalRefLengthRule(int MaxLength) : IOffboardingClaimRule
{
    public List<string> Check(OffboardingClaim item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingClaimScoreRangeRule(double Min, double Max) : IOffboardingClaimRule
{
    public List<string> Check(OffboardingClaim item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OffboardingClaimPhoneLengthRule(int MaxLength) : IOffboardingClaimRule
{
    public List<string> Check(OffboardingClaim item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingClaimRequiredPhoneRule(string Message) : IOffboardingClaimRule
{
    public List<string> Check(OffboardingClaim item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingClaimCodePrefixRule(string Prefix) : IOffboardingClaimRule
{
    public List<string> Check(OffboardingClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding claim.</summary>
public sealed record OffboardingClaimValidator(List<IOffboardingClaimRule> Rules)
{
    public List<string> Validate(OffboardingClaim item)
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

    public void RequireValid(OffboardingClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingClaim> items)
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

/// <summary>Functions of offboarding claim rules.</summary>
public static class OffboardingClaimRules
{
    public static OffboardingClaimValidator DefaultOffboardingClaimValidator()
    {
        return new OffboardingClaimValidator(
            new List<IOffboardingClaimRule>
            {
                new OffboardingClaimExternalRefLengthRule(120),
                new OffboardingClaimScoreRangeRule(6.1, 4370.5),
                new OffboardingClaimPhoneLengthRule(1000),
                new OffboardingClaimRequiredPhoneRule("offboarding claim needs this field"),
                new OffboardingClaimCodePrefixRule("OPS"),
            });
    }
}
