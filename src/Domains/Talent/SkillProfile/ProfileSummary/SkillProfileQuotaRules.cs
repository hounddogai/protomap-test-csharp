namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>A rule a skill profile quota must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileQuotaRule
{
    List<string> Check(SkillProfileQuota item);
}

public sealed record SkillProfileQuotaRequiredLabelRule(string Message) : ISkillProfileQuotaRule
{
    public List<string> Check(SkillProfileQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileQuotaLabelLengthRule(int MaxLength) : ISkillProfileQuotaRule
{
    public List<string> Check(SkillProfileQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileQuotaAmountRangeRule(double Min, double Max) : ISkillProfileQuotaRule
{
    public List<string> Check(SkillProfileQuota item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileQuotaRequiredCodeRule(string Message) : ISkillProfileQuotaRule
{
    public List<string> Check(SkillProfileQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile quota.</summary>
public sealed record SkillProfileQuotaValidator(List<ISkillProfileQuotaRule> Rules)
{
    public List<string> Validate(SkillProfileQuota item)
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

    public void RequireValid(SkillProfileQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileQuota> items)
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

/// <summary>Functions of skill profile quota rules.</summary>
public static class SkillProfileQuotaRules
{
    public static SkillProfileQuotaValidator DefaultSkillProfileQuotaValidator()
    {
        return new SkillProfileQuotaValidator(
            new List<ISkillProfileQuotaRule>
            {
                new SkillProfileQuotaRequiredLabelRule("skill profile quota needs a value"),
                new SkillProfileQuotaLabelLengthRule(86400),
                new SkillProfileQuotaAmountRangeRule(5.9, 2466.0),
                new SkillProfileQuotaRequiredCodeRule("skill profile quota needs an entry"),
            });
    }
}
