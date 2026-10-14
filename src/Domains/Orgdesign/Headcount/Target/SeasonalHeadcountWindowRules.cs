namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal headcount window must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHeadcountWindowRule
{
    List<string> Check(SeasonalHeadcountWindow item);
}

public sealed record SeasonalHeadcountWindowRequiredLabelRule(string Message) : ISeasonalHeadcountWindowRule
{
    public List<string> Check(SeasonalHeadcountWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHeadcountWindowRequiredSkillRule(string Message) : ISeasonalHeadcountWindowRule
{
    public List<string> Check(SeasonalHeadcountWindow item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHeadcountWindowAllowedSkillRule(List<string> Allowed) : ISeasonalHeadcountWindowRule
{
    public List<string> Check(SeasonalHeadcountWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHeadcountWindowJobCodeLengthRule(int MaxLength) : ISeasonalHeadcountWindowRule
{
    public List<string> Check(SeasonalHeadcountWindow item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHeadcountWindowMarginRangeRule(double Min, double Max) : ISeasonalHeadcountWindowRule
{
    public List<string> Check(SeasonalHeadcountWindow item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalHeadcountWindowWindowDaysRangeRule(int Min, int Max) : ISeasonalHeadcountWindowRule
{
    public List<string> Check(SeasonalHeadcountWindow item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal headcount window.</summary>
public sealed record SeasonalHeadcountWindowValidator(List<ISeasonalHeadcountWindowRule> Rules)
{
    public List<string> Validate(SeasonalHeadcountWindow item)
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

    public void RequireValid(SeasonalHeadcountWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHeadcountWindow> items)
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

/// <summary>Functions of seasonal headcount window rules.</summary>
public static class SeasonalHeadcountWindowRules
{
    public static SeasonalHeadcountWindowValidator DefaultSeasonalHeadcountWindowValidator()
    {
        return new SeasonalHeadcountWindowValidator(
            new List<ISeasonalHeadcountWindowRule>
            {
                new SeasonalHeadcountWindowRequiredLabelRule("seasonal headcount window needs an entry"),
                new SeasonalHeadcountWindowRequiredSkillRule("seasonal headcount window needs this field"),
                new SeasonalHeadcountWindowAllowedSkillRule(new List<string>()),
                new SeasonalHeadcountWindowJobCodeLengthRule(3600),
                new SeasonalHeadcountWindowMarginRangeRule(3.2, 3260.5),
                new SeasonalHeadcountWindowWindowDaysRangeRule(64, 64),
            });
    }
}
