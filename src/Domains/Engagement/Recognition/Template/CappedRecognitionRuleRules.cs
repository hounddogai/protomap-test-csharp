namespace Acme.Workforce.Engagement.Recognition.Template;

/// <summary>A rule a capped recognition rule must pass. Check returns its problems, or none.</summary>
public interface ICappedRecognitionRuleRule
{
    List<string> Check(CappedRecognitionRule item);
}

public sealed record CappedRecognitionRuleRequiredPhoneRule(string Message) : ICappedRecognitionRuleRule
{
    public List<string> Check(CappedRecognitionRule item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedRecognitionRuleTitleLengthRule(int MaxLength) : ICappedRecognitionRuleRule
{
    public List<string> Check(CappedRecognitionRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRecognitionRulePhoneLengthRule(int MaxLength) : ICappedRecognitionRuleRule
{
    public List<string> Check(CappedRecognitionRule item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRecognitionRuleCodeLengthRule(int MaxLength) : ICappedRecognitionRuleRule
{
    public List<string> Check(CappedRecognitionRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped recognition rule.</summary>
public sealed record CappedRecognitionRuleValidator(List<ICappedRecognitionRuleRule> Rules)
{
    public List<string> Validate(CappedRecognitionRule item)
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

    public void RequireValid(CappedRecognitionRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedRecognitionRule> items)
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

/// <summary>Functions of capped recognition rule rules.</summary>
public static class CappedRecognitionRuleRules
{
    public static CappedRecognitionRuleValidator DefaultCappedRecognitionRuleValidator()
    {
        return new CappedRecognitionRuleValidator(
            new List<ICappedRecognitionRuleRule>
            {
                new CappedRecognitionRuleRequiredPhoneRule("capped recognition rule needs a value"),
                new CappedRecognitionRuleTitleLengthRule(32),
                new CappedRecognitionRulePhoneLengthRule(3600),
                new CappedRecognitionRuleCodeLengthRule(1000),
            });
    }
}
