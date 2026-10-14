namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped pool agreement must pass. Check returns its problems, or none.</summary>
public interface ICappedPoolAgreementRule
{
    List<string> Check(CappedPoolAgreement item);
}

public sealed record CappedPoolAgreementDiscountRangeRule(double Min, double Max) : ICappedPoolAgreementRule
{
    public List<string> Check(CappedPoolAgreement item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedPoolAgreementAllowedCodeRule(List<string> Allowed) : ICappedPoolAgreementRule
{
    public List<string> Check(CappedPoolAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record CappedPoolAgreementSkillLengthRule(int MaxLength) : ICappedPoolAgreementRule
{
    public List<string> Check(CappedPoolAgreement item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pool agreement.</summary>
public sealed record CappedPoolAgreementValidator(List<ICappedPoolAgreementRule> Rules)
{
    public List<string> Validate(CappedPoolAgreement item)
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

    public void RequireValid(CappedPoolAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPoolAgreement> items)
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

/// <summary>Functions of capped pool agreement rules.</summary>
public static class CappedPoolAgreementRules
{
    public static CappedPoolAgreementValidator DefaultCappedPoolAgreementValidator()
    {
        return new CappedPoolAgreementValidator(
            new List<ICappedPoolAgreementRule>
            {
                new CappedPoolAgreementDiscountRangeRule(1.8, 262.1),
                new CappedPoolAgreementAllowedCodeRule(new List<string> { "global", "tiered", "capped" }),
                new CappedPoolAgreementSkillLengthRule(120),
            });
    }
}
