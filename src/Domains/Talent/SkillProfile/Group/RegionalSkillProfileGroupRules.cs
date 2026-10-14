namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional skill profile group must pass. Check returns its problems, or none.</summary>
public interface IRegionalSkillProfileGroupRule
{
    List<string> Check(RegionalSkillProfileGroup item);
}

public sealed record RegionalSkillProfileGroupDurationMinutesRangeRule(int Min, int Max) : IRegionalSkillProfileGroupRule
{
    public List<string> Check(RegionalSkillProfileGroup item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalSkillProfileGroupRequiredChannelRule(string Message) : IRegionalSkillProfileGroupRule
{
    public List<string> Check(RegionalSkillProfileGroup item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileGroupAllowedJobCodeRule(List<string> Allowed) : IRegionalSkillProfileGroupRule
{
    public List<string> Check(RegionalSkillProfileGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional skill profile group.</summary>
public sealed record RegionalSkillProfileGroupValidator(List<IRegionalSkillProfileGroupRule> Rules)
{
    public List<string> Validate(RegionalSkillProfileGroup item)
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

    public void RequireValid(RegionalSkillProfileGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSkillProfileGroup> items)
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

/// <summary>Functions of regional skill profile group rules.</summary>
public static class RegionalSkillProfileGroupRules
{
    public static RegionalSkillProfileGroupValidator DefaultRegionalSkillProfileGroupValidator()
    {
        return new RegionalSkillProfileGroupValidator(
            new List<IRegionalSkillProfileGroupRule>
            {
                new RegionalSkillProfileGroupDurationMinutesRangeRule(32, 255),
                new RegionalSkillProfileGroupRequiredChannelRule("regional skill profile group needs a value"),
                new RegionalSkillProfileGroupAllowedJobCodeRule(new List<string>()),
            });
    }
}
