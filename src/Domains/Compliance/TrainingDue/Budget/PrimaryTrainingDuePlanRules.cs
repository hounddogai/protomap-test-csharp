namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary training due plan must pass. Check returns its problems, or none.</summary>
public interface IPrimaryTrainingDuePlanRule
{
    List<string> Check(PrimaryTrainingDuePlan item);
}

public sealed record PrimaryTrainingDuePlanRequiredCodeRule(string Message) : IPrimaryTrainingDuePlanRule
{
    public List<string> Check(PrimaryTrainingDuePlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTrainingDuePlanThresholdRangeRule(double Min, double Max) : IPrimaryTrainingDuePlanRule
{
    public List<string> Check(PrimaryTrainingDuePlan item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryTrainingDuePlanAllowedTimezoneRule(List<string> Allowed) : IPrimaryTrainingDuePlanRule
{
    public List<string> Check(PrimaryTrainingDuePlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTrainingDuePlanCodeLengthRule(int MaxLength) : IPrimaryTrainingDuePlanRule
{
    public List<string> Check(PrimaryTrainingDuePlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTrainingDuePlanRequiredTitleRule(string Message) : IPrimaryTrainingDuePlanRule
{
    public List<string> Check(PrimaryTrainingDuePlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary training due plan.</summary>
public sealed record PrimaryTrainingDuePlanValidator(List<IPrimaryTrainingDuePlanRule> Rules)
{
    public List<string> Validate(PrimaryTrainingDuePlan item)
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

    public void RequireValid(PrimaryTrainingDuePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryTrainingDuePlan> items)
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

/// <summary>Functions of primary training due plan rules.</summary>
public static class PrimaryTrainingDuePlanRules
{
    public static PrimaryTrainingDuePlanValidator DefaultPrimaryTrainingDuePlanValidator()
    {
        return new PrimaryTrainingDuePlanValidator(
            new List<IPrimaryTrainingDuePlanRule>
            {
                new PrimaryTrainingDuePlanRequiredCodeRule("primary training due plan needs a value"),
                new PrimaryTrainingDuePlanThresholdRangeRule(0.6, 3248.1),
                new PrimaryTrainingDuePlanAllowedTimezoneRule(new List<string> { "global", "adaptive", "global" }),
                new PrimaryTrainingDuePlanCodeLengthRule(3600),
                new PrimaryTrainingDuePlanRequiredTitleRule("primary training due plan needs an entry"),
            });
    }
}
