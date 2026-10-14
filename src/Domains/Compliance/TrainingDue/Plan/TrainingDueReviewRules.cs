namespace Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>A rule a training due review must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueReviewRule
{
    List<string> Check(TrainingDueReview item);
}

public sealed record TrainingDueReviewActiveGuardRule(int Limit) : ITrainingDueReviewRule
{
    public List<string> Check(TrainingDueReview item)
    {
        if (item.Active && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueReviewRequiredTitleRule(string Message) : ITrainingDueReviewRule
{
    public List<string> Check(TrainingDueReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueReviewRequiredLabelRule(string Message) : ITrainingDueReviewRule
{
    public List<string> Check(TrainingDueReview item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueReviewLeaveDaysRangeRule(int Min, int Max) : ITrainingDueReviewRule
{
    public List<string> Check(TrainingDueReview item)
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

/// <summary>Runs every rule of a training due review.</summary>
public sealed record TrainingDueReviewValidator(List<ITrainingDueReviewRule> Rules)
{
    public List<string> Validate(TrainingDueReview item)
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

    public void RequireValid(TrainingDueReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueReview> items)
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

/// <summary>Functions of training due review rules.</summary>
public static class TrainingDueReviewRules
{
    public static TrainingDueReviewValidator DefaultTrainingDueReviewValidator()
    {
        return new TrainingDueReviewValidator(
            new List<ITrainingDueReviewRule>
            {
                new TrainingDueReviewActiveGuardRule(3600),
                new TrainingDueReviewRequiredTitleRule("training due review needs this field"),
                new TrainingDueReviewRequiredLabelRule("training due review needs a value"),
                new TrainingDueReviewLeaveDaysRangeRule(32, 120),
            });
    }
}
