namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a training due tier must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueTierRule
{
    List<string> Check(TrainingDueTier item);
}

public sealed record TrainingDueTierPinnedGuardRule(int Limit) : ITrainingDueTierRule
{
    public List<string> Check(TrainingDueTier item)
    {
        if (item.Pinned && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTierCodePrefixRule(string Prefix) : ITrainingDueTierRule
{
    public List<string> Check(TrainingDueTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTierRequiredCategoryRule(string Message) : ITrainingDueTierRule
{
    public List<string> Check(TrainingDueTier item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTierAllowedLabelRule(List<string> Allowed) : ITrainingDueTierRule
{
    public List<string> Check(TrainingDueTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTierRequiredDepartmentRule(string Message) : ITrainingDueTierRule
{
    public List<string> Check(TrainingDueTier item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due tier.</summary>
public sealed record TrainingDueTierValidator(List<ITrainingDueTierRule> Rules)
{
    public List<string> Validate(TrainingDueTier item)
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

    public void RequireValid(TrainingDueTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueTier> items)
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

/// <summary>Functions of training due tier rules.</summary>
public static class TrainingDueTierRules
{
    public static TrainingDueTierValidator DefaultTrainingDueTierValidator()
    {
        return new TrainingDueTierValidator(
            new List<ITrainingDueTierRule>
            {
                new TrainingDueTierPinnedGuardRule(120),
                new TrainingDueTierCodePrefixRule("X"),
                new TrainingDueTierRequiredCategoryRule("training due tier needs this field"),
                new TrainingDueTierAllowedLabelRule(new List<string> { "strict", "fallback", "scheduled" }),
                new TrainingDueTierRequiredDepartmentRule("training due tier needs a value"),
            });
    }
}
