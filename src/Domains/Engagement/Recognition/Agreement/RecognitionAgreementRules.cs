namespace Acme.Workforce.Engagement.Recognition.Agreement;

/// <summary>A rule a recognition agreement must pass. Check returns its problems, or none.</summary>
public interface IRecognitionAgreementRule
{
    List<string> Check(RecognitionAgreement item);
}

public sealed record RecognitionAgreementMinimumRangeRule(double Min, double Max) : IRecognitionAgreementRule
{
    public List<string> Check(RecognitionAgreement item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionAgreementRequiredLocaleRule(string Message) : IRecognitionAgreementRule
{
    public List<string> Check(RecognitionAgreement item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionAgreementRequiredLabelRule(string Message) : IRecognitionAgreementRule
{
    public List<string> Check(RecognitionAgreement item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition agreement.</summary>
public sealed record RecognitionAgreementValidator(List<IRecognitionAgreementRule> Rules)
{
    public List<string> Validate(RecognitionAgreement item)
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

    public void RequireValid(RecognitionAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionAgreement> items)
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

/// <summary>Functions of recognition agreement rules.</summary>
public static class RecognitionAgreementRules
{
    public static RecognitionAgreementValidator DefaultRecognitionAgreementValidator()
    {
        return new RecognitionAgreementValidator(
            new List<IRecognitionAgreementRule>
            {
                new RecognitionAgreementMinimumRangeRule(5.9, 3702.6),
                new RecognitionAgreementRequiredLocaleRule("recognition agreement needs an entry"),
                new RecognitionAgreementRequiredLabelRule("recognition agreement needs a value"),
            });
    }
}
