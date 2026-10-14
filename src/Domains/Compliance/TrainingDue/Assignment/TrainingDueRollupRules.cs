namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>A rule a training due rollup must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueRollupRule
{
    List<string> Check(TrainingDueRollup item);
}

public sealed record TrainingDueRollupRequiredLabelRule(string Message) : ITrainingDueRollupRule
{
    public List<string> Check(TrainingDueRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueRollupCodeLengthRule(int MaxLength) : ITrainingDueRollupRule
{
    public List<string> Check(TrainingDueRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueRollupRequiredJobCodeRule(string Message) : ITrainingDueRollupRule
{
    public List<string> Check(TrainingDueRollup item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueRollupLabelLengthRule(int MaxLength) : ITrainingDueRollupRule
{
    public List<string> Check(TrainingDueRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueRollupCodePrefixRule(string Prefix) : ITrainingDueRollupRule
{
    public List<string> Check(TrainingDueRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due rollup.</summary>
public sealed record TrainingDueRollupValidator(List<ITrainingDueRollupRule> Rules)
{
    public List<string> Validate(TrainingDueRollup item)
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

    public void RequireValid(TrainingDueRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueRollup> items)
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

/// <summary>Functions of training due rollup rules.</summary>
public static class TrainingDueRollupRules
{
    public static TrainingDueRollupValidator DefaultTrainingDueRollupValidator()
    {
        return new TrainingDueRollupValidator(
            new List<ITrainingDueRollupRule>
            {
                new TrainingDueRollupRequiredLabelRule("training due rollup needs this field"),
                new TrainingDueRollupCodeLengthRule(80),
                new TrainingDueRollupRequiredJobCodeRule("training due rollup needs this field"),
                new TrainingDueRollupLabelLengthRule(120),
                new TrainingDueRollupCodePrefixRule("Q"),
            });
    }
}
