namespace Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>A rule a primary recognition policy must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRecognitionPolicyRule
{
    List<string> Check(PrimaryRecognitionPolicy item);
}

public sealed record PrimaryRecognitionPolicyRetriesRangeRule(int Min, int Max) : IPrimaryRecognitionPolicyRule
{
    public List<string> Check(PrimaryRecognitionPolicy item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRecognitionPolicySourceLengthRule(int MaxLength) : IPrimaryRecognitionPolicyRule
{
    public List<string> Check(PrimaryRecognitionPolicy item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRecognitionPolicyRequiredOwnerIdRule(string Message) : IPrimaryRecognitionPolicyRule
{
    public List<string> Check(PrimaryRecognitionPolicy item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRecognitionPolicyBatchSizeRangeRule(int Min, int Max) : IPrimaryRecognitionPolicyRule
{
    public List<string> Check(PrimaryRecognitionPolicy item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary recognition policy.</summary>
public sealed record PrimaryRecognitionPolicyValidator(List<IPrimaryRecognitionPolicyRule> Rules)
{
    public List<string> Validate(PrimaryRecognitionPolicy item)
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

    public void RequireValid(PrimaryRecognitionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRecognitionPolicy> items)
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

/// <summary>Functions of primary recognition policy rules.</summary>
public static class PrimaryRecognitionPolicyRules
{
    public static PrimaryRecognitionPolicyValidator DefaultPrimaryRecognitionPolicyValidator()
    {
        return new PrimaryRecognitionPolicyValidator(
            new List<IPrimaryRecognitionPolicyRule>
            {
                new PrimaryRecognitionPolicyRetriesRangeRule(16, 255),
                new PrimaryRecognitionPolicySourceLengthRule(64),
                new PrimaryRecognitionPolicyRequiredOwnerIdRule("primary recognition policy needs an entry"),
                new PrimaryRecognitionPolicyBatchSizeRangeRule(16, 80),
            });
    }
}
