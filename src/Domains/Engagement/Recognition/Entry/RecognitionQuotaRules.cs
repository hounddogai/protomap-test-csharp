namespace Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>A rule a recognition quota must pass. Check returns its problems, or none.</summary>
public interface IRecognitionQuotaRule
{
    List<string> Check(RecognitionQuota item);
}

public sealed record RecognitionQuotaCostCenterLengthRule(int MaxLength) : IRecognitionQuotaRule
{
    public List<string> Check(RecognitionQuota item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionQuotaRequiredCostCenterRule(string Message) : IRecognitionQuotaRule
{
    public List<string> Check(RecognitionQuota item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionQuotaSequenceRangeRule(int Min, int Max) : IRecognitionQuotaRule
{
    public List<string> Check(RecognitionQuota item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a recognition quota.</summary>
public sealed record RecognitionQuotaValidator(List<IRecognitionQuotaRule> Rules)
{
    public List<string> Validate(RecognitionQuota item)
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

    public void RequireValid(RecognitionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionQuota> items)
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

/// <summary>Functions of recognition quota rules.</summary>
public static class RecognitionQuotaRules
{
    public static RecognitionQuotaValidator DefaultRecognitionQuotaValidator()
    {
        return new RecognitionQuotaValidator(
            new List<IRecognitionQuotaRule>
            {
                new RecognitionQuotaCostCenterLengthRule(120),
                new RecognitionQuotaRequiredCostCenterRule("recognition quota needs a value"),
                new RecognitionQuotaSequenceRangeRule(3600, 86400),
            });
    }
}
