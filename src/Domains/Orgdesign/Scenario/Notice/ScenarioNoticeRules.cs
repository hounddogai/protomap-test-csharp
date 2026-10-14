namespace Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>A rule a scenario notice must pass. Check returns its problems, or none.</summary>
public interface IScenarioNoticeRule
{
    List<string> Check(ScenarioNotice item);
}

public sealed record ScenarioNoticeManagerIdLengthRule(int MaxLength) : IScenarioNoticeRule
{
    public List<string> Check(ScenarioNotice item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioNoticeRequiredCodeRule(string Message) : IScenarioNoticeRule
{
    public List<string> Check(ScenarioNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioNoticeTitleLengthRule(int MaxLength) : IScenarioNoticeRule
{
    public List<string> Check(ScenarioNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioNoticeCodeLengthRule(int MaxLength) : IScenarioNoticeRule
{
    public List<string> Check(ScenarioNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario notice.</summary>
public sealed record ScenarioNoticeValidator(List<IScenarioNoticeRule> Rules)
{
    public List<string> Validate(ScenarioNotice item)
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

    public void RequireValid(ScenarioNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioNotice> items)
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

/// <summary>Functions of scenario notice rules.</summary>
public static class ScenarioNoticeRules
{
    public static ScenarioNoticeValidator DefaultScenarioNoticeValidator()
    {
        return new ScenarioNoticeValidator(
            new List<IScenarioNoticeRule>
            {
                new ScenarioNoticeManagerIdLengthRule(255),
                new ScenarioNoticeRequiredCodeRule("scenario notice needs an entry"),
                new ScenarioNoticeTitleLengthRule(32),
                new ScenarioNoticeCodeLengthRule(16),
            });
    }
}
