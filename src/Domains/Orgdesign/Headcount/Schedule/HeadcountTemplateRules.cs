namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>A rule a headcount template must pass. Check returns its problems, or none.</summary>
public interface IHeadcountTemplateRule
{
    List<string> Check(HeadcountTemplate item);
}

public sealed record HeadcountTemplateTitleLengthRule(int MaxLength) : IHeadcountTemplateRule
{
    public List<string> Check(HeadcountTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTemplateRequiredGradeRule(string Message) : IHeadcountTemplateRule
{
    public List<string> Check(HeadcountTemplate item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTemplateCodePrefixRule(string Prefix) : IHeadcountTemplateRule
{
    public List<string> Check(HeadcountTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTemplateTenureMonthsRangeRule(int Min, int Max) : IHeadcountTemplateRule
{
    public List<string> Check(HeadcountTemplate item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTemplateRequiredTeamRule(string Message) : IHeadcountTemplateRule
{
    public List<string> Check(HeadcountTemplate item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTemplateTeamLengthRule(int MaxLength) : IHeadcountTemplateRule
{
    public List<string> Check(HeadcountTemplate item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount template.</summary>
public sealed record HeadcountTemplateValidator(List<IHeadcountTemplateRule> Rules)
{
    public List<string> Validate(HeadcountTemplate item)
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

    public void RequireValid(HeadcountTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountTemplate> items)
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

/// <summary>Functions of headcount template rules.</summary>
public static class HeadcountTemplateRules
{
    public static HeadcountTemplateValidator DefaultHeadcountTemplateValidator()
    {
        return new HeadcountTemplateValidator(
            new List<IHeadcountTemplateRule>
            {
                new HeadcountTemplateTitleLengthRule(120),
                new HeadcountTemplateRequiredGradeRule("headcount template needs a value"),
                new HeadcountTemplateCodePrefixRule("X"),
                new HeadcountTemplateTenureMonthsRangeRule(32, 86400),
                new HeadcountTemplateRequiredTeamRule("headcount template needs a value"),
                new HeadcountTemplateTeamLengthRule(80),
            });
    }
}
