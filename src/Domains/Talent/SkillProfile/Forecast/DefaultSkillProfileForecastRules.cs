namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default skill profile forecast must pass. Check returns its problems, or none.</summary>
public interface IDefaultSkillProfileForecastRule
{
    List<string> Check(DefaultSkillProfileForecast item);
}

public sealed record DefaultSkillProfileForecastTeamLengthRule(int MaxLength) : IDefaultSkillProfileForecastRule
{
    public List<string> Check(DefaultSkillProfileForecast item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileForecastAllowedTeamRule(List<string> Allowed) : IDefaultSkillProfileForecastRule
{
    public List<string> Check(DefaultSkillProfileForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileForecastRequiredCodeRule(string Message) : IDefaultSkillProfileForecastRule
{
    public List<string> Check(DefaultSkillProfileForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileForecastCodeLengthRule(int MaxLength) : IDefaultSkillProfileForecastRule
{
    public List<string> Check(DefaultSkillProfileForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileForecastRequiredTeamRule(string Message) : IDefaultSkillProfileForecastRule
{
    public List<string> Check(DefaultSkillProfileForecast item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default skill profile forecast.</summary>
public sealed record DefaultSkillProfileForecastValidator(List<IDefaultSkillProfileForecastRule> Rules)
{
    public List<string> Validate(DefaultSkillProfileForecast item)
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

    public void RequireValid(DefaultSkillProfileForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSkillProfileForecast> items)
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

/// <summary>Functions of default skill profile forecast rules.</summary>
public static class DefaultSkillProfileForecastRules
{
    public static DefaultSkillProfileForecastValidator DefaultDefaultSkillProfileForecastValidator()
    {
        return new DefaultSkillProfileForecastValidator(
            new List<IDefaultSkillProfileForecastRule>
            {
                new DefaultSkillProfileForecastTeamLengthRule(80),
                new DefaultSkillProfileForecastAllowedTeamRule(new List<string>()),
                new DefaultSkillProfileForecastRequiredCodeRule("default skill profile forecast needs a value"),
                new DefaultSkillProfileForecastCodeLengthRule(80),
                new DefaultSkillProfileForecastRequiredTeamRule("default skill profile forecast needs this field"),
            });
    }
}
