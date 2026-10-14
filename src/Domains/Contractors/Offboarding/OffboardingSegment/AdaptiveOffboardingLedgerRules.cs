namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>A rule a adaptive offboarding ledger must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveOffboardingLedgerRule
{
    List<string> Check(AdaptiveOffboardingLedger item);
}

public sealed record AdaptiveOffboardingLedgerRequiredRegionRule(string Message) : IAdaptiveOffboardingLedgerRule
{
    public List<string> Check(AdaptiveOffboardingLedger item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingLedgerMaximumRangeRule(double Min, double Max) : IAdaptiveOffboardingLedgerRule
{
    public List<string> Check(AdaptiveOffboardingLedger item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingLedgerCodePrefixRule(string Prefix) : IAdaptiveOffboardingLedgerRule
{
    public List<string> Check(AdaptiveOffboardingLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingLedgerNameLengthRule(int MaxLength) : IAdaptiveOffboardingLedgerRule
{
    public List<string> Check(AdaptiveOffboardingLedger item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingLedgerCodeLengthRule(int MaxLength) : IAdaptiveOffboardingLedgerRule
{
    public List<string> Check(AdaptiveOffboardingLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingLedgerSkillLengthRule(int MaxLength) : IAdaptiveOffboardingLedgerRule
{
    public List<string> Check(AdaptiveOffboardingLedger item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive offboarding ledger.</summary>
public sealed record AdaptiveOffboardingLedgerValidator(List<IAdaptiveOffboardingLedgerRule> Rules)
{
    public List<string> Validate(AdaptiveOffboardingLedger item)
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

    public void RequireValid(AdaptiveOffboardingLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveOffboardingLedger> items)
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

/// <summary>Functions of adaptive offboarding ledger rules.</summary>
public static class AdaptiveOffboardingLedgerRules
{
    public static AdaptiveOffboardingLedgerValidator DefaultAdaptiveOffboardingLedgerValidator()
    {
        return new AdaptiveOffboardingLedgerValidator(
            new List<IAdaptiveOffboardingLedgerRule>
            {
                new AdaptiveOffboardingLedgerRequiredRegionRule("adaptive offboarding ledger needs this field"),
                new AdaptiveOffboardingLedgerMaximumRangeRule(1.8, 2395.9),
                new AdaptiveOffboardingLedgerCodePrefixRule("ACM"),
                new AdaptiveOffboardingLedgerNameLengthRule(32),
                new AdaptiveOffboardingLedgerCodeLengthRule(3600),
                new AdaptiveOffboardingLedgerSkillLengthRule(64),
            });
    }
}
