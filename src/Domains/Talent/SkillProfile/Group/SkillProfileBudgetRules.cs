namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile budget must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileBudgetRule
{
    List<string> Check(SkillProfileBudget item);
}

public sealed record SkillProfileBudgetRequiredCodeRule(string Message) : ISkillProfileBudgetRule
{
    public List<string> Check(SkillProfileBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileBudgetRetriesRangeRule(int Min, int Max) : ISkillProfileBudgetRule
{
    public List<string> Check(SkillProfileBudget item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileBudgetApprovedGuardRule(double Limit) : ISkillProfileBudgetRule
{
    public List<string> Check(SkillProfileBudget item)
    {
        if (item.Approved && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileBudgetAllowedTitleRule(List<string> Allowed) : ISkillProfileBudgetRule
{
    public List<string> Check(SkillProfileBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileBudgetCodeLengthRule(int MaxLength) : ISkillProfileBudgetRule
{
    public List<string> Check(SkillProfileBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile budget.</summary>
public sealed record SkillProfileBudgetValidator(List<ISkillProfileBudgetRule> Rules)
{
    public List<string> Validate(SkillProfileBudget item)
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

    public void RequireValid(SkillProfileBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileBudget> items)
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

/// <summary>Functions of skill profile budget rules.</summary>
public static class SkillProfileBudgetRules
{
    public static SkillProfileBudgetValidator DefaultSkillProfileBudgetValidator()
    {
        return new SkillProfileBudgetValidator(
            new List<ISkillProfileBudgetRule>
            {
                new SkillProfileBudgetRequiredCodeRule("skill profile budget needs an entry"),
                new SkillProfileBudgetRetriesRangeRule(32, 86400),
                new SkillProfileBudgetApprovedGuardRule(6.0),
                new SkillProfileBudgetAllowedTitleRule(new List<string>()),
                new SkillProfileBudgetCodeLengthRule(1000),
            });
    }
}
