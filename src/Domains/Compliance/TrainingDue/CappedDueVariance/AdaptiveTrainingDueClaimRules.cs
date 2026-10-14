namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>A rule a adaptive training due claim must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTrainingDueClaimRule
{
    List<string> Check(AdaptiveTrainingDueClaim item);
}

public sealed record AdaptiveTrainingDueClaimRevisionRangeRule(int Min, int Max) : IAdaptiveTrainingDueClaimRule
{
    public List<string> Check(AdaptiveTrainingDueClaim item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveTrainingDueClaimMinimumRangeRule(double Min, double Max) : IAdaptiveTrainingDueClaimRule
{
    public List<string> Check(AdaptiveTrainingDueClaim item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveTrainingDueClaimRequiredCodeRule(string Message) : IAdaptiveTrainingDueClaimRule
{
    public List<string> Check(AdaptiveTrainingDueClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTrainingDueClaimBatchSizeRangeRule(int Min, int Max) : IAdaptiveTrainingDueClaimRule
{
    public List<string> Check(AdaptiveTrainingDueClaim item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTrainingDueClaimRequiredPhoneRule(string Message) : IAdaptiveTrainingDueClaimRule
{
    public List<string> Check(AdaptiveTrainingDueClaim item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTrainingDueClaimLabelLengthRule(int MaxLength) : IAdaptiveTrainingDueClaimRule
{
    public List<string> Check(AdaptiveTrainingDueClaim item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive training due claim.</summary>
public sealed record AdaptiveTrainingDueClaimValidator(List<IAdaptiveTrainingDueClaimRule> Rules)
{
    public List<string> Validate(AdaptiveTrainingDueClaim item)
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

    public void RequireValid(AdaptiveTrainingDueClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTrainingDueClaim> items)
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

/// <summary>Functions of adaptive training due claim rules.</summary>
public static class AdaptiveTrainingDueClaimRules
{
    public static AdaptiveTrainingDueClaimValidator DefaultAdaptiveTrainingDueClaimValidator()
    {
        return new AdaptiveTrainingDueClaimValidator(
            new List<IAdaptiveTrainingDueClaimRule>
            {
                new AdaptiveTrainingDueClaimRevisionRangeRule(1000, 1000),
                new AdaptiveTrainingDueClaimMinimumRangeRule(3.7, 4912.9),
                new AdaptiveTrainingDueClaimRequiredCodeRule("adaptive training due claim needs a value"),
                new AdaptiveTrainingDueClaimBatchSizeRangeRule(120, 1000),
                new AdaptiveTrainingDueClaimRequiredPhoneRule("adaptive training due claim needs an entry"),
                new AdaptiveTrainingDueClaimLabelLengthRule(255),
            });
    }
}
