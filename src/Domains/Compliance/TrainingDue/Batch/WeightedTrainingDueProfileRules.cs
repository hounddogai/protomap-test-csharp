namespace Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>A rule a weighted training due profile must pass. Check returns its problems, or none.</summary>
public interface IWeightedTrainingDueProfileRule
{
    List<string> Check(WeightedTrainingDueProfile item);
}

public sealed record WeightedTrainingDueProfileRequiredTitleRule(string Message) : IWeightedTrainingDueProfileRule
{
    public List<string> Check(WeightedTrainingDueProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueProfileCodeLengthRule(int MaxLength) : IWeightedTrainingDueProfileRule
{
    public List<string> Check(WeightedTrainingDueProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueProfileRequiredSkillRule(string Message) : IWeightedTrainingDueProfileRule
{
    public List<string> Check(WeightedTrainingDueProfile item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueProfileReviewerLengthRule(int MaxLength) : IWeightedTrainingDueProfileRule
{
    public List<string> Check(WeightedTrainingDueProfile item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted training due profile.</summary>
public sealed record WeightedTrainingDueProfileValidator(List<IWeightedTrainingDueProfileRule> Rules)
{
    public List<string> Validate(WeightedTrainingDueProfile item)
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

    public void RequireValid(WeightedTrainingDueProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedTrainingDueProfile> items)
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

/// <summary>Functions of weighted training due profile rules.</summary>
public static class WeightedTrainingDueProfileRules
{
    public static WeightedTrainingDueProfileValidator DefaultWeightedTrainingDueProfileValidator()
    {
        return new WeightedTrainingDueProfileValidator(
            new List<IWeightedTrainingDueProfileRule>
            {
                new WeightedTrainingDueProfileRequiredTitleRule("weighted training due profile needs this field"),
                new WeightedTrainingDueProfileCodeLengthRule(120),
                new WeightedTrainingDueProfileRequiredSkillRule("weighted training due profile needs a value"),
                new WeightedTrainingDueProfileReviewerLengthRule(86400),
            });
    }
}
