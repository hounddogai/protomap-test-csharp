namespace Acme.Workforce.Absence.Policy.Profile;

/// <summary>A rule a default policy variance must pass. Check returns its problems, or none.</summary>
public interface IDefaultPolicyVarianceRule
{
    List<string> Check(DefaultPolicyVariance item);
}

public sealed record DefaultPolicyVarianceRequiredSkillRule(string Message) : IDefaultPolicyVarianceRule
{
    public List<string> Check(DefaultPolicyVariance item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyVarianceDiscountRangeRule(double Min, double Max) : IDefaultPolicyVarianceRule
{
    public List<string> Check(DefaultPolicyVariance item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyVarianceLabelLengthRule(int MaxLength) : IDefaultPolicyVarianceRule
{
    public List<string> Check(DefaultPolicyVariance item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default policy variance.</summary>
public sealed record DefaultPolicyVarianceValidator(List<IDefaultPolicyVarianceRule> Rules)
{
    public List<string> Validate(DefaultPolicyVariance item)
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

    public void RequireValid(DefaultPolicyVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPolicyVariance> items)
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

/// <summary>Functions of default policy variance rules.</summary>
public static class DefaultPolicyVarianceRules
{
    public static DefaultPolicyVarianceValidator DefaultDefaultPolicyVarianceValidator()
    {
        return new DefaultPolicyVarianceValidator(
            new List<IDefaultPolicyVarianceRule>
            {
                new DefaultPolicyVarianceRequiredSkillRule("default policy variance needs this field"),
                new DefaultPolicyVarianceDiscountRangeRule(6.6, 135.0),
                new DefaultPolicyVarianceLabelLengthRule(86400),
            });
    }
}
