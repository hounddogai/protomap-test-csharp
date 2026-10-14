namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched recognition draft must pass. Check returns its problems, or none.</summary>
public interface IBatchedRecognitionDraftRule
{
    List<string> Check(BatchedRecognitionDraft item);
}

public sealed record BatchedRecognitionDraftCodePrefixRule(string Prefix) : IBatchedRecognitionDraftRule
{
    public List<string> Check(BatchedRecognitionDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRecognitionDraftCodeLengthRule(int MaxLength) : IBatchedRecognitionDraftRule
{
    public List<string> Check(BatchedRecognitionDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRecognitionDraftSkillLengthRule(int MaxLength) : IBatchedRecognitionDraftRule
{
    public List<string> Check(BatchedRecognitionDraft item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRecognitionDraftAllowedPhoneRule(List<string> Allowed) : IBatchedRecognitionDraftRule
{
    public List<string> Check(BatchedRecognitionDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRecognitionDraftBudgetRangeRule(double Min, double Max) : IBatchedRecognitionDraftRule
{
    public List<string> Check(BatchedRecognitionDraft item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a batched recognition draft.</summary>
public sealed record BatchedRecognitionDraftValidator(List<IBatchedRecognitionDraftRule> Rules)
{
    public List<string> Validate(BatchedRecognitionDraft item)
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

    public void RequireValid(BatchedRecognitionDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRecognitionDraft> items)
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

/// <summary>Functions of batched recognition draft rules.</summary>
public static class BatchedRecognitionDraftRules
{
    public static BatchedRecognitionDraftValidator DefaultBatchedRecognitionDraftValidator()
    {
        return new BatchedRecognitionDraftValidator(
            new List<IBatchedRecognitionDraftRule>
            {
                new BatchedRecognitionDraftCodePrefixRule("OPS"),
                new BatchedRecognitionDraftCodeLengthRule(16),
                new BatchedRecognitionDraftSkillLengthRule(86400),
                new BatchedRecognitionDraftAllowedPhoneRule(new List<string> { "tiered" }),
                new BatchedRecognitionDraftBudgetRangeRule(4.6, 4441.4),
            });
    }
}
