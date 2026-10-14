namespace Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>A rule a pulse survey notice must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyNoticeRule
{
    List<string> Check(PulseSurveyNotice item);
}

public sealed record PulseSurveyNoticeRequiredTitleRule(string Message) : IPulseSurveyNoticeRule
{
    public List<string> Check(PulseSurveyNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyNoticeQuantityRangeRule(int Min, int Max) : IPulseSurveyNoticeRule
{
    public List<string> Check(PulseSurveyNotice item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyNoticeTitleLengthRule(int MaxLength) : IPulseSurveyNoticeRule
{
    public List<string> Check(PulseSurveyNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyNoticeRequiredDescriptionRule(string Message) : IPulseSurveyNoticeRule
{
    public List<string> Check(PulseSurveyNotice item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey notice.</summary>
public sealed record PulseSurveyNoticeValidator(List<IPulseSurveyNoticeRule> Rules)
{
    public List<string> Validate(PulseSurveyNotice item)
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

    public void RequireValid(PulseSurveyNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyNotice> items)
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

/// <summary>Functions of pulse survey notice rules.</summary>
public static class PulseSurveyNoticeRules
{
    public static PulseSurveyNoticeValidator DefaultPulseSurveyNoticeValidator()
    {
        return new PulseSurveyNoticeValidator(
            new List<IPulseSurveyNoticeRule>
            {
                new PulseSurveyNoticeRequiredTitleRule("pulse survey notice needs this field"),
                new PulseSurveyNoticeQuantityRangeRule(255, 86400),
                new PulseSurveyNoticeTitleLengthRule(32),
                new PulseSurveyNoticeRequiredDescriptionRule("pulse survey notice needs a value"),
            });
    }
}
