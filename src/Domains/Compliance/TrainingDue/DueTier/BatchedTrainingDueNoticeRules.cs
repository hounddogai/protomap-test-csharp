namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

/// <summary>A rule a batched training due notice must pass. Check returns its problems, or none.</summary>
public interface IBatchedTrainingDueNoticeRule
{
    List<string> Check(BatchedTrainingDueNotice item);
}

public sealed record BatchedTrainingDueNoticeRequiredSalaryBandRule(string Message) : IBatchedTrainingDueNoticeRule
{
    public List<string> Check(BatchedTrainingDueNotice item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTrainingDueNoticeRequiredNameRule(string Message) : IBatchedTrainingDueNoticeRule
{
    public List<string> Check(BatchedTrainingDueNotice item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTrainingDueNoticeNameLengthRule(int MaxLength) : IBatchedTrainingDueNoticeRule
{
    public List<string> Check(BatchedTrainingDueNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTrainingDueNoticeBudgetRangeRule(double Min, double Max) : IBatchedTrainingDueNoticeRule
{
    public List<string> Check(BatchedTrainingDueNotice item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched training due notice.</summary>
public sealed record BatchedTrainingDueNoticeValidator(List<IBatchedTrainingDueNoticeRule> Rules)
{
    public List<string> Validate(BatchedTrainingDueNotice item)
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

    public void RequireValid(BatchedTrainingDueNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTrainingDueNotice> items)
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

/// <summary>Functions of batched training due notice rules.</summary>
public static class BatchedTrainingDueNoticeRules
{
    public static BatchedTrainingDueNoticeValidator DefaultBatchedTrainingDueNoticeValidator()
    {
        return new BatchedTrainingDueNoticeValidator(
            new List<IBatchedTrainingDueNoticeRule>
            {
                new BatchedTrainingDueNoticeRequiredSalaryBandRule("batched training due notice needs this field"),
                new BatchedTrainingDueNoticeRequiredNameRule("batched training due notice needs an entry"),
                new BatchedTrainingDueNoticeNameLengthRule(1000),
                new BatchedTrainingDueNoticeBudgetRangeRule(5.7, 3259.6),
            });
    }
}
