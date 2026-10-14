namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global career path rule must pass. Check returns its problems, or none.</summary>
public interface IGlobalCareerPathRuleRule
{
    List<string> Check(GlobalCareerPathRule item);
}

public sealed record GlobalCareerPathRuleAllowedNameRule(List<string> Allowed) : IGlobalCareerPathRuleRule
{
    public List<string> Check(GlobalCareerPathRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record GlobalCareerPathRuleLeaveDaysRangeRule(int Min, int Max) : IGlobalCareerPathRuleRule
{
    public List<string> Check(GlobalCareerPathRule item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalCareerPathRuleRequiredSegmentRule(string Message) : IGlobalCareerPathRuleRule
{
    public List<string> Check(GlobalCareerPathRule item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalCareerPathRuleRequiredLabelRule(string Message) : IGlobalCareerPathRuleRule
{
    public List<string> Check(GlobalCareerPathRule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global career path rule.</summary>
public sealed record GlobalCareerPathRuleValidator(List<IGlobalCareerPathRuleRule> Rules)
{
    public List<string> Validate(GlobalCareerPathRule item)
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

    public void RequireValid(GlobalCareerPathRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalCareerPathRule> items)
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

/// <summary>Functions of global career path rule rules.</summary>
public static class GlobalCareerPathRuleRules
{
    public static GlobalCareerPathRuleValidator DefaultGlobalCareerPathRuleValidator()
    {
        return new GlobalCareerPathRuleValidator(
            new List<IGlobalCareerPathRuleRule>
            {
                new GlobalCareerPathRuleAllowedNameRule(new List<string>()),
                new GlobalCareerPathRuleLeaveDaysRangeRule(16, 80),
                new GlobalCareerPathRuleRequiredSegmentRule("global career path rule needs an entry"),
                new GlobalCareerPathRuleRequiredLabelRule("global career path rule needs an entry"),
            });
    }
}
