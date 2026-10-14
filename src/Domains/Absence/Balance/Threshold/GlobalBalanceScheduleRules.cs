namespace Acme.Workforce.Absence.Balance.Threshold;

/// <summary>A rule a global balance schedule must pass. Check returns its problems, or none.</summary>
public interface IGlobalBalanceScheduleRule
{
    List<string> Check(GlobalBalanceSchedule item);
}

public sealed record GlobalBalanceScheduleTeamLengthRule(int MaxLength) : IGlobalBalanceScheduleRule
{
    public List<string> Check(GlobalBalanceSchedule item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBalanceScheduleRequiredTeamRule(string Message) : IGlobalBalanceScheduleRule
{
    public List<string> Check(GlobalBalanceSchedule item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalBalanceScheduleRequiredSkillRule(string Message) : IGlobalBalanceScheduleRule
{
    public List<string> Check(GlobalBalanceSchedule item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global balance schedule.</summary>
public sealed record GlobalBalanceScheduleValidator(List<IGlobalBalanceScheduleRule> Rules)
{
    public List<string> Validate(GlobalBalanceSchedule item)
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

    public void RequireValid(GlobalBalanceSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalBalanceSchedule> items)
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

/// <summary>Functions of global balance schedule rules.</summary>
public static class GlobalBalanceScheduleRules
{
    public static GlobalBalanceScheduleValidator DefaultGlobalBalanceScheduleValidator()
    {
        return new GlobalBalanceScheduleValidator(
            new List<IGlobalBalanceScheduleRule>
            {
                new GlobalBalanceScheduleTeamLengthRule(120),
                new GlobalBalanceScheduleRequiredTeamRule("global balance schedule needs an entry"),
                new GlobalBalanceScheduleRequiredSkillRule("global balance schedule needs this field"),
            });
    }
}
