namespace Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>A rule a recognition rule must pass. Check returns its problems, or none.</summary>
public interface IRecognitionRuleRule
{
    List<string> Check(RecognitionRule item);
}

public sealed record RecognitionRuleRequiredCodeRule(string Message) : IRecognitionRuleRule
{
    public List<string> Check(RecognitionRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRuleTenureMonthsRangeRule(int Min, int Max) : IRecognitionRuleRule
{
    public List<string> Check(RecognitionRule item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionRuleTitleLengthRule(int MaxLength) : IRecognitionRuleRule
{
    public List<string> Check(RecognitionRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRuleStartsAtRangeRule(int Min, int Max) : IRecognitionRuleRule
{
    public List<string> Check(RecognitionRule item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionRuleSalaryBandLengthRule(int MaxLength) : IRecognitionRuleRule
{
    public List<string> Check(RecognitionRule item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition rule.</summary>
public sealed record RecognitionRuleValidator(List<IRecognitionRuleRule> Rules)
{
    public List<string> Validate(RecognitionRule item)
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

    public void RequireValid(RecognitionRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionRule> items)
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

/// <summary>Functions of recognition rule rules.</summary>
public static class RecognitionRuleRules
{
    public static RecognitionRuleValidator DefaultRecognitionRuleValidator()
    {
        return new RecognitionRuleValidator(
            new List<IRecognitionRuleRule>
            {
                new RecognitionRuleRequiredCodeRule("recognition rule needs a value"),
                new RecognitionRuleTenureMonthsRangeRule(120, 86400),
                new RecognitionRuleTitleLengthRule(120),
                new RecognitionRuleStartsAtRangeRule(64, 3600),
                new RecognitionRuleSalaryBandLengthRule(120),
            });
    }
}
