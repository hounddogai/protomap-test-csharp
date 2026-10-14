namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding agreement must pass. Check returns its problems, or none.</summary>
public interface IOffboardingAgreementRule
{
    List<string> Check(OffboardingAgreement item);
}

public sealed record OffboardingAgreementCodePrefixRule(string Prefix) : IOffboardingAgreementRule
{
    public List<string> Check(OffboardingAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAgreementAllowedNameRule(List<string> Allowed) : IOffboardingAgreementRule
{
    public List<string> Check(OffboardingAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAgreementLabelLengthRule(int MaxLength) : IOffboardingAgreementRule
{
    public List<string> Check(OffboardingAgreement item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingAgreementLeaveDaysRangeRule(int Min, int Max) : IOffboardingAgreementRule
{
    public List<string> Check(OffboardingAgreement item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding agreement.</summary>
public sealed record OffboardingAgreementValidator(List<IOffboardingAgreementRule> Rules)
{
    public List<string> Validate(OffboardingAgreement item)
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

    public void RequireValid(OffboardingAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingAgreement> items)
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

/// <summary>Functions of offboarding agreement rules.</summary>
public static class OffboardingAgreementRules
{
    public static OffboardingAgreementValidator DefaultOffboardingAgreementValidator()
    {
        return new OffboardingAgreementValidator(
            new List<IOffboardingAgreementRule>
            {
                new OffboardingAgreementCodePrefixRule("Q"),
                new OffboardingAgreementAllowedNameRule(new List<string> { "regional", "fallback", "weighted" }),
                new OffboardingAgreementLabelLengthRule(3600),
                new OffboardingAgreementLeaveDaysRangeRule(255, 86400),
            });
    }
}
