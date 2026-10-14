namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

/// <summary>A rule a scheduled training due draft must pass. Check returns its problems, or none.</summary>
public interface IScheduledTrainingDueDraftRule
{
    List<string> Check(ScheduledTrainingDueDraft item);
}

public sealed record ScheduledTrainingDueDraftCodePrefixRule(string Prefix) : IScheduledTrainingDueDraftRule
{
    public List<string> Check(ScheduledTrainingDueDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTrainingDueDraftReviewerLengthRule(int MaxLength) : IScheduledTrainingDueDraftRule
{
    public List<string> Check(ScheduledTrainingDueDraft item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTrainingDueDraftCapacityRangeRule(int Min, int Max) : IScheduledTrainingDueDraftRule
{
    public List<string> Check(ScheduledTrainingDueDraft item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledTrainingDueDraftRequiredReviewerRule(string Message) : IScheduledTrainingDueDraftRule
{
    public List<string> Check(ScheduledTrainingDueDraft item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTrainingDueDraftManagerIdLengthRule(int MaxLength) : IScheduledTrainingDueDraftRule
{
    public List<string> Check(ScheduledTrainingDueDraft item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled training due draft.</summary>
public sealed record ScheduledTrainingDueDraftValidator(List<IScheduledTrainingDueDraftRule> Rules)
{
    public List<string> Validate(ScheduledTrainingDueDraft item)
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

    public void RequireValid(ScheduledTrainingDueDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTrainingDueDraft> items)
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

/// <summary>Functions of scheduled training due draft rules.</summary>
public static class ScheduledTrainingDueDraftRules
{
    public static ScheduledTrainingDueDraftValidator DefaultScheduledTrainingDueDraftValidator()
    {
        return new ScheduledTrainingDueDraftValidator(
            new List<IScheduledTrainingDueDraftRule>
            {
                new ScheduledTrainingDueDraftCodePrefixRule("Q"),
                new ScheduledTrainingDueDraftReviewerLengthRule(64),
                new ScheduledTrainingDueDraftCapacityRangeRule(16, 32),
                new ScheduledTrainingDueDraftRequiredReviewerRule("scheduled training due draft needs an entry"),
                new ScheduledTrainingDueDraftManagerIdLengthRule(32),
            });
    }
}
