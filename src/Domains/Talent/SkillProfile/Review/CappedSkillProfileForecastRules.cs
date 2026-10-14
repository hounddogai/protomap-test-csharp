namespace Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>A rule a capped skill profile forecast must pass. Check returns its problems, or none.</summary>
public interface ICappedSkillProfileForecastRule
{
    List<string> Check(CappedSkillProfileForecast item);
}

public sealed record CappedSkillProfileForecastEndsAtRangeRule(int Min, int Max) : ICappedSkillProfileForecastRule
{
    public List<string> Check(CappedSkillProfileForecast item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedSkillProfileForecastRequiredLabelRule(string Message) : ICappedSkillProfileForecastRule
{
    public List<string> Check(CappedSkillProfileForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileForecastRequiredCodeRule(string Message) : ICappedSkillProfileForecastRule
{
    public List<string> Check(CappedSkillProfileForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileForecastLabelLengthRule(int MaxLength) : ICappedSkillProfileForecastRule
{
    public List<string> Check(CappedSkillProfileForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped skill profile forecast.</summary>
public sealed record CappedSkillProfileForecastValidator(List<ICappedSkillProfileForecastRule> Rules)
{
    public List<string> Validate(CappedSkillProfileForecast item)
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

    public void RequireValid(CappedSkillProfileForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSkillProfileForecast> items)
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

/// <summary>Functions of capped skill profile forecast rules.</summary>
public static class CappedSkillProfileForecastRules
{
    public static CappedSkillProfileForecastValidator DefaultCappedSkillProfileForecastValidator()
    {
        return new CappedSkillProfileForecastValidator(
            new List<ICappedSkillProfileForecastRule>
            {
                new CappedSkillProfileForecastEndsAtRangeRule(64, 3600),
                new CappedSkillProfileForecastRequiredLabelRule("capped skill profile forecast needs an entry"),
                new CappedSkillProfileForecastRequiredCodeRule("capped skill profile forecast needs a value"),
                new CappedSkillProfileForecastLabelLengthRule(80),
            });
    }
}
