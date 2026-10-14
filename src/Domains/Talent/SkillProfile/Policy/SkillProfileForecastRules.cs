namespace Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>A rule a skill profile forecast must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileForecastRule
{
    List<string> Check(SkillProfileForecast item);
}

public sealed record SkillProfileForecastRequiredNameRule(string Message) : ISkillProfileForecastRule
{
    public List<string> Check(SkillProfileForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileForecastNameLengthRule(int MaxLength) : ISkillProfileForecastRule
{
    public List<string> Check(SkillProfileForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileForecastCodePrefixRule(string Prefix) : ISkillProfileForecastRule
{
    public List<string> Check(SkillProfileForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile forecast.</summary>
public sealed record SkillProfileForecastValidator(List<ISkillProfileForecastRule> Rules)
{
    public List<string> Validate(SkillProfileForecast item)
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

    public void RequireValid(SkillProfileForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileForecast> items)
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

/// <summary>Functions of skill profile forecast rules.</summary>
public static class SkillProfileForecastRules
{
    public static SkillProfileForecastValidator DefaultSkillProfileForecastValidator()
    {
        return new SkillProfileForecastValidator(
            new List<ISkillProfileForecastRule>
            {
                new SkillProfileForecastRequiredNameRule("skill profile forecast needs this field"),
                new SkillProfileForecastNameLengthRule(120),
                new SkillProfileForecastCodePrefixRule("Q"),
            });
    }
}
