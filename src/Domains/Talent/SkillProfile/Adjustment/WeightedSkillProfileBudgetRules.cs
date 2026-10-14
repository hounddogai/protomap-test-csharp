namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted skill profile budget must pass. Check returns its problems, or none.</summary>
public interface IWeightedSkillProfileBudgetRule
{
    List<string> Check(WeightedSkillProfileBudget item);
}

public sealed record WeightedSkillProfileBudgetMaximumRangeRule(double Min, double Max) : IWeightedSkillProfileBudgetRule
{
    public List<string> Check(WeightedSkillProfileBudget item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedSkillProfileBudgetTitleLengthRule(int MaxLength) : IWeightedSkillProfileBudgetRule
{
    public List<string> Check(WeightedSkillProfileBudget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSkillProfileBudgetCodePrefixRule(string Prefix) : IWeightedSkillProfileBudgetRule
{
    public List<string> Check(WeightedSkillProfileBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSkillProfileBudgetAllowedCodeRule(List<string> Allowed) : IWeightedSkillProfileBudgetRule
{
    public List<string> Check(WeightedSkillProfileBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSkillProfileBudgetRequiredCodeRule(string Message) : IWeightedSkillProfileBudgetRule
{
    public List<string> Check(WeightedSkillProfileBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted skill profile budget.</summary>
public sealed record WeightedSkillProfileBudgetValidator(List<IWeightedSkillProfileBudgetRule> Rules)
{
    public List<string> Validate(WeightedSkillProfileBudget item)
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

    public void RequireValid(WeightedSkillProfileBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedSkillProfileBudget> items)
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

/// <summary>Functions of weighted skill profile budget rules.</summary>
public static class WeightedSkillProfileBudgetRules
{
    public static WeightedSkillProfileBudgetValidator DefaultWeightedSkillProfileBudgetValidator()
    {
        return new WeightedSkillProfileBudgetValidator(
            new List<IWeightedSkillProfileBudgetRule>
            {
                new WeightedSkillProfileBudgetMaximumRangeRule(3.7, 380.7),
                new WeightedSkillProfileBudgetTitleLengthRule(16),
                new WeightedSkillProfileBudgetCodePrefixRule("REF"),
                new WeightedSkillProfileBudgetAllowedCodeRule(new List<string>()),
                new WeightedSkillProfileBudgetRequiredCodeRule("weighted skill profile budget needs an entry"),
            });
    }
}
