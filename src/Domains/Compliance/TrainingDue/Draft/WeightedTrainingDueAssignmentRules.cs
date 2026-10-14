namespace Acme.Workforce.Compliance.TrainingDue.Draft;

/// <summary>A rule a weighted training due assignment must pass. Check returns its problems, or none.</summary>
public interface IWeightedTrainingDueAssignmentRule
{
    List<string> Check(WeightedTrainingDueAssignment item);
}

public sealed record WeightedTrainingDueAssignmentCodeLengthRule(int MaxLength) : IWeightedTrainingDueAssignmentRule
{
    public List<string> Check(WeightedTrainingDueAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAssignmentLabelLengthRule(int MaxLength) : IWeightedTrainingDueAssignmentRule
{
    public List<string> Check(WeightedTrainingDueAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAssignmentCodePrefixRule(string Prefix) : IWeightedTrainingDueAssignmentRule
{
    public List<string> Check(WeightedTrainingDueAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAssignmentRequiredLabelRule(string Message) : IWeightedTrainingDueAssignmentRule
{
    public List<string> Check(WeightedTrainingDueAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAssignmentRequiredCodeRule(string Message) : IWeightedTrainingDueAssignmentRule
{
    public List<string> Check(WeightedTrainingDueAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted training due assignment.</summary>
public sealed record WeightedTrainingDueAssignmentValidator(List<IWeightedTrainingDueAssignmentRule> Rules)
{
    public List<string> Validate(WeightedTrainingDueAssignment item)
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

    public void RequireValid(WeightedTrainingDueAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedTrainingDueAssignment> items)
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

/// <summary>Functions of weighted training due assignment rules.</summary>
public static class WeightedTrainingDueAssignmentRules
{
    public static WeightedTrainingDueAssignmentValidator DefaultWeightedTrainingDueAssignmentValidator()
    {
        return new WeightedTrainingDueAssignmentValidator(
            new List<IWeightedTrainingDueAssignmentRule>
            {
                new WeightedTrainingDueAssignmentCodeLengthRule(80),
                new WeightedTrainingDueAssignmentLabelLengthRule(16),
                new WeightedTrainingDueAssignmentCodePrefixRule("Q"),
                new WeightedTrainingDueAssignmentRequiredLabelRule(
                    "weighted training due assignment needs this field"),
                new WeightedTrainingDueAssignmentRequiredCodeRule(
                    "weighted training due assignment needs this field"),
            });
    }
}
