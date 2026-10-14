namespace Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>A rule a fallback training due batch must pass. Check returns its problems, or none.</summary>
public interface IFallbackTrainingDueBatchRule
{
    List<string> Check(FallbackTrainingDueBatch item);
}

public sealed record FallbackTrainingDueBatchTitleLengthRule(int MaxLength) : IFallbackTrainingDueBatchRule
{
    public List<string> Check(FallbackTrainingDueBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueBatchCodePrefixRule(string Prefix) : IFallbackTrainingDueBatchRule
{
    public List<string> Check(FallbackTrainingDueBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueBatchRequiredTimezoneRule(string Message) : IFallbackTrainingDueBatchRule
{
    public List<string> Check(FallbackTrainingDueBatch item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback training due batch.</summary>
public sealed record FallbackTrainingDueBatchValidator(List<IFallbackTrainingDueBatchRule> Rules)
{
    public List<string> Validate(FallbackTrainingDueBatch item)
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

    public void RequireValid(FallbackTrainingDueBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackTrainingDueBatch> items)
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

/// <summary>Functions of fallback training due batch rules.</summary>
public static class FallbackTrainingDueBatchRules
{
    public static FallbackTrainingDueBatchValidator DefaultFallbackTrainingDueBatchValidator()
    {
        return new FallbackTrainingDueBatchValidator(
            new List<IFallbackTrainingDueBatchRule>
            {
                new FallbackTrainingDueBatchTitleLengthRule(1000),
                new FallbackTrainingDueBatchCodePrefixRule("Q"),
                new FallbackTrainingDueBatchRequiredTimezoneRule("fallback training due batch needs this field"),
            });
    }
}
