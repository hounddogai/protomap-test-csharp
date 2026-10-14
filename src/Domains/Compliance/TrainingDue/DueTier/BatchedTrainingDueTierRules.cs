namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched training due tier must pass. Check returns its problems, or none.</summary>
public interface IBatchedTrainingDueTierRule
{
    List<string> Check(BatchedTrainingDueTier item);
}

public sealed record BatchedTrainingDueTierCodeLengthRule(int MaxLength) : IBatchedTrainingDueTierRule
{
    public List<string> Check(BatchedTrainingDueTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTrainingDueTierRequiredLabelRule(string Message) : IBatchedTrainingDueTierRule
{
    public List<string> Check(BatchedTrainingDueTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTrainingDueTierLabelLengthRule(int MaxLength) : IBatchedTrainingDueTierRule
{
    public List<string> Check(BatchedTrainingDueTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTrainingDueTierLeaveDaysRangeRule(int Min, int Max) : IBatchedTrainingDueTierRule
{
    public List<string> Check(BatchedTrainingDueTier item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedTrainingDueTierAllowedCodeRule(List<string> Allowed) : IBatchedTrainingDueTierRule
{
    public List<string> Check(BatchedTrainingDueTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched training due tier.</summary>
public sealed record BatchedTrainingDueTierValidator(List<IBatchedTrainingDueTierRule> Rules)
{
    public List<string> Validate(BatchedTrainingDueTier item)
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

    public void RequireValid(BatchedTrainingDueTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTrainingDueTier> items)
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

/// <summary>Functions of batched training due tier rules.</summary>
public static class BatchedTrainingDueTierRules
{
    public static BatchedTrainingDueTierValidator DefaultBatchedTrainingDueTierValidator()
    {
        return new BatchedTrainingDueTierValidator(
            new List<IBatchedTrainingDueTierRule>
            {
                new BatchedTrainingDueTierCodeLengthRule(120),
                new BatchedTrainingDueTierRequiredLabelRule("batched training due tier needs a value"),
                new BatchedTrainingDueTierLabelLengthRule(3600),
                new BatchedTrainingDueTierLeaveDaysRangeRule(80, 80),
                new BatchedTrainingDueTierAllowedCodeRule(new List<string>()),
            });
    }
}
