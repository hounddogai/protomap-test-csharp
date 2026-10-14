namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>A rule a primary recognition variance must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRecognitionVarianceRule
{
    List<string> Check(PrimaryRecognitionVariance item);
}

public sealed record PrimaryRecognitionVarianceRequiredCodeRule(string Message) : IPrimaryRecognitionVarianceRule
{
    public List<string> Check(PrimaryRecognitionVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRecognitionVarianceApprovedGuardRule(double Limit) : IPrimaryRecognitionVarianceRule
{
    public List<string> Check(PrimaryRecognitionVariance item)
    {
        if (item.Approved && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRecognitionVarianceRequiredTitleRule(string Message) : IPrimaryRecognitionVarianceRule
{
    public List<string> Check(PrimaryRecognitionVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRecognitionVarianceLocaleLengthRule(int MaxLength) : IPrimaryRecognitionVarianceRule
{
    public List<string> Check(PrimaryRecognitionVariance item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary recognition variance.</summary>
public sealed record PrimaryRecognitionVarianceValidator(List<IPrimaryRecognitionVarianceRule> Rules)
{
    public List<string> Validate(PrimaryRecognitionVariance item)
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

    public void RequireValid(PrimaryRecognitionVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRecognitionVariance> items)
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

/// <summary>Functions of primary recognition variance rules.</summary>
public static class PrimaryRecognitionVarianceRules
{
    public static PrimaryRecognitionVarianceValidator DefaultPrimaryRecognitionVarianceValidator()
    {
        return new PrimaryRecognitionVarianceValidator(
            new List<IPrimaryRecognitionVarianceRule>
            {
                new PrimaryRecognitionVarianceRequiredCodeRule("primary recognition variance needs this field"),
                new PrimaryRecognitionVarianceApprovedGuardRule(4.8),
                new PrimaryRecognitionVarianceRequiredTitleRule("primary recognition variance needs a value"),
                new PrimaryRecognitionVarianceLocaleLengthRule(1000),
            });
    }
}
