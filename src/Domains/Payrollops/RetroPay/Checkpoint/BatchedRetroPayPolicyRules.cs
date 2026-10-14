namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>A rule a batched retro pay policy must pass. Check returns its problems, or none.</summary>
public interface IBatchedRetroPayPolicyRule
{
    List<string> Check(BatchedRetroPayPolicy item);
}

public sealed record BatchedRetroPayPolicyRatingRangeRule(double Min, double Max) : IBatchedRetroPayPolicyRule
{
    public List<string> Check(BatchedRetroPayPolicy item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedRetroPayPolicyCodePrefixRule(string Prefix) : IBatchedRetroPayPolicyRule
{
    public List<string> Check(BatchedRetroPayPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRetroPayPolicyExpiresAtRangeRule(int Min, int Max) : IBatchedRetroPayPolicyRule
{
    public List<string> Check(BatchedRetroPayPolicy item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRetroPayPolicyRequiredTitleRule(string Message) : IBatchedRetroPayPolicyRule
{
    public List<string> Check(BatchedRetroPayPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedRetroPayPolicyWindowDaysRangeRule(int Min, int Max) : IBatchedRetroPayPolicyRule
{
    public List<string> Check(BatchedRetroPayPolicy item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched retro pay policy.</summary>
public sealed record BatchedRetroPayPolicyValidator(List<IBatchedRetroPayPolicyRule> Rules)
{
    public List<string> Validate(BatchedRetroPayPolicy item)
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

    public void RequireValid(BatchedRetroPayPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRetroPayPolicy> items)
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

/// <summary>Functions of batched retro pay policy rules.</summary>
public static class BatchedRetroPayPolicyRules
{
    public static BatchedRetroPayPolicyValidator DefaultBatchedRetroPayPolicyValidator()
    {
        return new BatchedRetroPayPolicyValidator(
            new List<IBatchedRetroPayPolicyRule>
            {
                new BatchedRetroPayPolicyRatingRangeRule(0.4, 3434.0),
                new BatchedRetroPayPolicyCodePrefixRule("OPS"),
                new BatchedRetroPayPolicyExpiresAtRangeRule(16, 120),
                new BatchedRetroPayPolicyRequiredTitleRule("batched retro pay policy needs this field"),
                new BatchedRetroPayPolicyWindowDaysRangeRule(120, 120),
            });
    }
}
