namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped skill profile adjustment must pass. Check returns its problems, or none.</summary>
public interface ICappedSkillProfileAdjustmentRule
{
    List<string> Check(CappedSkillProfileAdjustment item);
}

public sealed record CappedSkillProfileAdjustmentAllowedManagerIdRule(List<string> Allowed) : ICappedSkillProfileAdjustmentRule
{
    public List<string> Check(CappedSkillProfileAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is expired" };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileAdjustmentRequiredNameRule(string Message) : ICappedSkillProfileAdjustmentRule
{
    public List<string> Check(CappedSkillProfileAdjustment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileAdjustmentRequiredCodeRule(string Message) : ICappedSkillProfileAdjustmentRule
{
    public List<string> Check(CappedSkillProfileAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileAdjustmentSalaryBandLengthRule(int MaxLength) : ICappedSkillProfileAdjustmentRule
{
    public List<string> Check(CappedSkillProfileAdjustment item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileAdjustmentQuantityRangeRule(int Min, int Max) : ICappedSkillProfileAdjustmentRule
{
    public List<string> Check(CappedSkillProfileAdjustment item)
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

/// <summary>Runs every rule of a capped skill profile adjustment.</summary>
public sealed record CappedSkillProfileAdjustmentValidator(List<ICappedSkillProfileAdjustmentRule> Rules)
{
    public List<string> Validate(CappedSkillProfileAdjustment item)
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

    public void RequireValid(CappedSkillProfileAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSkillProfileAdjustment> items)
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

/// <summary>Functions of capped skill profile adjustment rules.</summary>
public static class CappedSkillProfileAdjustmentRules
{
    public static CappedSkillProfileAdjustmentValidator DefaultCappedSkillProfileAdjustmentValidator()
    {
        return new CappedSkillProfileAdjustmentValidator(
            new List<ICappedSkillProfileAdjustmentRule>
            {
                new CappedSkillProfileAdjustmentAllowedManagerIdRule(new List<string> { "global", "scheduled" }),
                new CappedSkillProfileAdjustmentRequiredNameRule("capped skill profile adjustment needs this field"),
                new CappedSkillProfileAdjustmentRequiredCodeRule("capped skill profile adjustment needs a value"),
                new CappedSkillProfileAdjustmentSalaryBandLengthRule(32),
                new CappedSkillProfileAdjustmentQuantityRangeRule(255, 3600),
            });
    }
}
