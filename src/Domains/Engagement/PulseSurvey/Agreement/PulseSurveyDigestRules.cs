namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>A rule a pulse survey digest must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyDigestRule
{
    List<string> Check(PulseSurveyDigest item);
}

public sealed record PulseSurveyDigestWeightRangeRule(double Min, double Max) : IPulseSurveyDigestRule
{
    public List<string> Check(PulseSurveyDigest item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyDigestCodeLengthRule(int MaxLength) : IPulseSurveyDigestRule
{
    public List<string> Check(PulseSurveyDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyDigestRequiredTitleRule(string Message) : IPulseSurveyDigestRule
{
    public List<string> Check(PulseSurveyDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey digest.</summary>
public sealed record PulseSurveyDigestValidator(List<IPulseSurveyDigestRule> Rules)
{
    public List<string> Validate(PulseSurveyDigest item)
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

    public void RequireValid(PulseSurveyDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyDigest> items)
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

/// <summary>Functions of pulse survey digest rules.</summary>
public static class PulseSurveyDigestRules
{
    public static PulseSurveyDigestValidator DefaultPulseSurveyDigestValidator()
    {
        return new PulseSurveyDigestValidator(
            new List<IPulseSurveyDigestRule>
            {
                new PulseSurveyDigestWeightRangeRule(8.9, 4369.5),
                new PulseSurveyDigestCodeLengthRule(1000),
                new PulseSurveyDigestRequiredTitleRule("pulse survey digest needs this field"),
            });
    }
}
