namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>A rule a adaptive training due plan must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTrainingDuePlanRule
{
    List<string> Check(AdaptiveTrainingDuePlan item);
}

public sealed record AdaptiveTrainingDuePlanRequiredCodeRule(string Message) : IAdaptiveTrainingDuePlanRule
{
    public List<string> Check(AdaptiveTrainingDuePlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTrainingDuePlanCodePrefixRule(string Prefix) : IAdaptiveTrainingDuePlanRule
{
    public List<string> Check(AdaptiveTrainingDuePlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTrainingDuePlanWindowDaysRangeRule(int Min, int Max) : IAdaptiveTrainingDuePlanRule
{
    public List<string> Check(AdaptiveTrainingDuePlan item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTrainingDuePlanCodeLengthRule(int MaxLength) : IAdaptiveTrainingDuePlanRule
{
    public List<string> Check(AdaptiveTrainingDuePlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive training due plan.</summary>
public sealed record AdaptiveTrainingDuePlanValidator(List<IAdaptiveTrainingDuePlanRule> Rules)
{
    public List<string> Validate(AdaptiveTrainingDuePlan item)
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

    public void RequireValid(AdaptiveTrainingDuePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTrainingDuePlan> items)
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

/// <summary>Functions of adaptive training due plan rules.</summary>
public static class AdaptiveTrainingDuePlanRules
{
    public static AdaptiveTrainingDuePlanValidator DefaultAdaptiveTrainingDuePlanValidator()
    {
        return new AdaptiveTrainingDuePlanValidator(
            new List<IAdaptiveTrainingDuePlanRule>
            {
                new AdaptiveTrainingDuePlanRequiredCodeRule("adaptive training due plan needs a value"),
                new AdaptiveTrainingDuePlanCodePrefixRule("ACM"),
                new AdaptiveTrainingDuePlanWindowDaysRangeRule(64, 1000),
                new AdaptiveTrainingDuePlanCodeLengthRule(3600),
            });
    }
}
