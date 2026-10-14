namespace Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>A rule a pending skill profile forecast must pass. Check returns its problems, or none.</summary>
public interface IPendingSkillProfileForecastRule
{
    List<string> Check(PendingSkillProfileForecast item);
}

public sealed record PendingSkillProfileForecastCodeLengthRule(int MaxLength) : IPendingSkillProfileForecastRule
{
    public List<string> Check(PendingSkillProfileForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileForecastAttemptsRangeRule(int Min, int Max) : IPendingSkillProfileForecastRule
{
    public List<string> Check(PendingSkillProfileForecast item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingSkillProfileForecastRequiredCodeRule(string Message) : IPendingSkillProfileForecastRule
{
    public List<string> Check(PendingSkillProfileForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileForecastSummaryLengthRule(int MaxLength) : IPendingSkillProfileForecastRule
{
    public List<string> Check(PendingSkillProfileForecast item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending skill profile forecast.</summary>
public sealed record PendingSkillProfileForecastValidator(List<IPendingSkillProfileForecastRule> Rules)
{
    public List<string> Validate(PendingSkillProfileForecast item)
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

    public void RequireValid(PendingSkillProfileForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSkillProfileForecast> items)
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

/// <summary>Functions of pending skill profile forecast rules.</summary>
public static class PendingSkillProfileForecastRules
{
    public static PendingSkillProfileForecastValidator DefaultPendingSkillProfileForecastValidator()
    {
        return new PendingSkillProfileForecastValidator(
            new List<IPendingSkillProfileForecastRule>
            {
                new PendingSkillProfileForecastCodeLengthRule(80),
                new PendingSkillProfileForecastAttemptsRangeRule(64, 120),
                new PendingSkillProfileForecastRequiredCodeRule("pending skill profile forecast needs this field"),
                new PendingSkillProfileForecastSummaryLengthRule(1000),
            });
    }
}
