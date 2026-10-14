namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>A rule a training due override must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueOverrideRule
{
    List<string> Check(TrainingDueOverride item);
}

public sealed record TrainingDueOverrideDiscountRangeRule(double Min, double Max) : ITrainingDueOverrideRule
{
    public List<string> Check(TrainingDueOverride item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueOverrideRequiredCodeRule(string Message) : ITrainingDueOverrideRule
{
    public List<string> Check(TrainingDueOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueOverrideManagerIdLengthRule(int MaxLength) : ITrainingDueOverrideRule
{
    public List<string> Check(TrainingDueOverride item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueOverrideCodePrefixRule(string Prefix) : ITrainingDueOverrideRule
{
    public List<string> Check(TrainingDueOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueOverrideRequiredLabelRule(string Message) : ITrainingDueOverrideRule
{
    public List<string> Check(TrainingDueOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueOverrideApprovedGuardRule(double Limit) : ITrainingDueOverrideRule
{
    public List<string> Check(TrainingDueOverride item)
    {
        if (item.Approved && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due override.</summary>
public sealed record TrainingDueOverrideValidator(List<ITrainingDueOverrideRule> Rules)
{
    public List<string> Validate(TrainingDueOverride item)
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

    public void RequireValid(TrainingDueOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueOverride> items)
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

/// <summary>Functions of training due override rules.</summary>
public static class TrainingDueOverrideRules
{
    public static TrainingDueOverrideValidator DefaultTrainingDueOverrideValidator()
    {
        return new TrainingDueOverrideValidator(
            new List<ITrainingDueOverrideRule>
            {
                new TrainingDueOverrideDiscountRangeRule(7.4, 2412.0),
                new TrainingDueOverrideRequiredCodeRule("training due override needs an entry"),
                new TrainingDueOverrideManagerIdLengthRule(16),
                new TrainingDueOverrideCodePrefixRule("ACM"),
                new TrainingDueOverrideRequiredLabelRule("training due override needs an entry"),
                new TrainingDueOverrideApprovedGuardRule(6.7),
            });
    }
}
