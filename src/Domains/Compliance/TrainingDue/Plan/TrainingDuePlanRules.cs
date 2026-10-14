namespace Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>A rule a training due plan must pass. Check returns its problems, or none.</summary>
public interface ITrainingDuePlanRule
{
    List<string> Check(TrainingDuePlan item);
}

public sealed record TrainingDuePlanRequiredCodeRule(string Message) : ITrainingDuePlanRule
{
    public List<string> Check(TrainingDuePlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDuePlanLeaveDaysRangeRule(int Min, int Max) : ITrainingDuePlanRule
{
    public List<string> Check(TrainingDuePlan item)
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

public sealed record TrainingDuePlanCodePrefixRule(string Prefix) : ITrainingDuePlanRule
{
    public List<string> Check(TrainingDuePlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDuePlanVisibleGuardRule(double Limit) : ITrainingDuePlanRule
{
    public List<string> Check(TrainingDuePlan item)
    {
        if (item.Visible && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due plan.</summary>
public sealed record TrainingDuePlanValidator(List<ITrainingDuePlanRule> Rules)
{
    public List<string> Validate(TrainingDuePlan item)
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

    public void RequireValid(TrainingDuePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDuePlan> items)
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

/// <summary>Functions of training due plan rules.</summary>
public static class TrainingDuePlanRules
{
    public static TrainingDuePlanValidator DefaultTrainingDuePlanValidator()
    {
        return new TrainingDuePlanValidator(
            new List<ITrainingDuePlanRule>
            {
                new TrainingDuePlanRequiredCodeRule("training due plan needs an entry"),
                new TrainingDuePlanLeaveDaysRangeRule(32, 120),
                new TrainingDuePlanCodePrefixRule("ACM"),
                new TrainingDuePlanVisibleGuardRule(1.1),
            });
    }
}
