namespace Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>A rule a skill profile agreement must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileAgreementRule
{
    List<string> Check(SkillProfileAgreement item);
}

public sealed record SkillProfileAgreementApprovedGuardRule(int Limit) : ISkillProfileAgreementRule
{
    public List<string> Check(SkillProfileAgreement item)
    {
        if (item.Approved && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileAgreementRequiredLabelRule(string Message) : ISkillProfileAgreementRule
{
    public List<string> Check(SkillProfileAgreement item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileAgreementCodePrefixRule(string Prefix) : ISkillProfileAgreementRule
{
    public List<string> Check(SkillProfileAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileAgreementQuantityRangeRule(int Min, int Max) : ISkillProfileAgreementRule
{
    public List<string> Check(SkillProfileAgreement item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a skill profile agreement.</summary>
public sealed record SkillProfileAgreementValidator(List<ISkillProfileAgreementRule> Rules)
{
    public List<string> Validate(SkillProfileAgreement item)
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

    public void RequireValid(SkillProfileAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileAgreement> items)
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

/// <summary>Functions of skill profile agreement rules.</summary>
public static class SkillProfileAgreementRules
{
    public static SkillProfileAgreementValidator DefaultSkillProfileAgreementValidator()
    {
        return new SkillProfileAgreementValidator(
            new List<ISkillProfileAgreementRule>
            {
                new SkillProfileAgreementApprovedGuardRule(80),
                new SkillProfileAgreementRequiredLabelRule("skill profile agreement needs an entry"),
                new SkillProfileAgreementCodePrefixRule("REF"),
                new SkillProfileAgreementQuantityRangeRule(80, 3600),
            });
    }
}
