namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a training due baseline must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueBaselineRule
{
    List<string> Check(TrainingDueBaseline item);
}

public sealed record TrainingDueBaselineLabelLengthRule(int MaxLength) : ITrainingDueBaselineRule
{
    public List<string> Check(TrainingDueBaseline item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueBaselineCodePrefixRule(string Prefix) : ITrainingDueBaselineRule
{
    public List<string> Check(TrainingDueBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueBaselineAllowedGradeRule(List<string> Allowed) : ITrainingDueBaselineRule
{
    public List<string> Check(TrainingDueBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due baseline.</summary>
public sealed record TrainingDueBaselineValidator(List<ITrainingDueBaselineRule> Rules)
{
    public List<string> Validate(TrainingDueBaseline item)
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

    public void RequireValid(TrainingDueBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueBaseline> items)
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

/// <summary>Functions of training due baseline rules.</summary>
public static class TrainingDueBaselineRules
{
    public static TrainingDueBaselineValidator DefaultTrainingDueBaselineValidator()
    {
        return new TrainingDueBaselineValidator(
            new List<ITrainingDueBaselineRule>
            {
                new TrainingDueBaselineLabelLengthRule(1000),
                new TrainingDueBaselineCodePrefixRule("Q"),
                new TrainingDueBaselineAllowedGradeRule(new List<string> { "lenient", "batched" }),
            });
    }
}
