namespace Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>A rule a lenient recognition batch must pass. Check returns its problems, or none.</summary>
public interface ILenientRecognitionBatchRule
{
    List<string> Check(LenientRecognitionBatch item);
}

public sealed record LenientRecognitionBatchUnitLengthRule(int MaxLength) : ILenientRecognitionBatchRule
{
    public List<string> Check(LenientRecognitionBatch item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRecognitionBatchCodeLengthRule(int MaxLength) : ILenientRecognitionBatchRule
{
    public List<string> Check(LenientRecognitionBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRecognitionBatchContactEmailLengthRule(int MaxLength) : ILenientRecognitionBatchRule
{
    public List<string> Check(LenientRecognitionBatch item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient recognition batch.</summary>
public sealed record LenientRecognitionBatchValidator(List<ILenientRecognitionBatchRule> Rules)
{
    public List<string> Validate(LenientRecognitionBatch item)
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

    public void RequireValid(LenientRecognitionBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRecognitionBatch> items)
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

/// <summary>Functions of lenient recognition batch rules.</summary>
public static class LenientRecognitionBatchRules
{
    public static LenientRecognitionBatchValidator DefaultLenientRecognitionBatchValidator()
    {
        return new LenientRecognitionBatchValidator(
            new List<ILenientRecognitionBatchRule>
            {
                new LenientRecognitionBatchUnitLengthRule(80),
                new LenientRecognitionBatchCodeLengthRule(32),
                new LenientRecognitionBatchContactEmailLengthRule(16),
            });
    }
}
