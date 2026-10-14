namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile baseline must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileBaselineRule
{
    List<string> Check(SkillProfileBaseline item);
}

public sealed record SkillProfileBaselineBadgeLengthRule(int MaxLength) : ISkillProfileBaselineRule
{
    public List<string> Check(SkillProfileBaseline item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileBaselineAllowedBadgeRule(List<string> Allowed) : ISkillProfileBaselineRule
{
    public List<string> Check(SkillProfileBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is expired" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileBaselineTitleLengthRule(int MaxLength) : ISkillProfileBaselineRule
{
    public List<string> Check(SkillProfileBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileBaselineAmountRangeRule(double Min, double Max) : ISkillProfileBaselineRule
{
    public List<string> Check(SkillProfileBaseline item)
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

public sealed record SkillProfileBaselineRequiredCodeRule(string Message) : ISkillProfileBaselineRule
{
    public List<string> Check(SkillProfileBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile baseline.</summary>
public sealed record SkillProfileBaselineValidator(List<ISkillProfileBaselineRule> Rules)
{
    public List<string> Validate(SkillProfileBaseline item)
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

    public void RequireValid(SkillProfileBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileBaseline> items)
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

/// <summary>Functions of skill profile baseline rules.</summary>
public static class SkillProfileBaselineRules
{
    public static SkillProfileBaselineValidator DefaultSkillProfileBaselineValidator()
    {
        return new SkillProfileBaselineValidator(
            new List<ISkillProfileBaselineRule>
            {
                new SkillProfileBaselineBadgeLengthRule(80),
                new SkillProfileBaselineAllowedBadgeRule(new List<string> { "pending", "adaptive" }),
                new SkillProfileBaselineTitleLengthRule(255),
                new SkillProfileBaselineAmountRangeRule(4.9, 1990.6),
                new SkillProfileBaselineRequiredCodeRule("skill profile baseline needs a value"),
            });
    }
}
