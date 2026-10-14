namespace Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>A rule a adjustment threshold must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentThresholdRule
{
    List<string> Check(AdjustmentThreshold item);
}

public sealed record AdjustmentThresholdSourceLengthRule(int MaxLength) : IAdjustmentThresholdRule
{
    public List<string> Check(AdjustmentThreshold item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentThresholdCodeLengthRule(int MaxLength) : IAdjustmentThresholdRule
{
    public List<string> Check(AdjustmentThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentThresholdRequiredJobCodeRule(string Message) : IAdjustmentThresholdRule
{
    public List<string> Check(AdjustmentThreshold item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment threshold.</summary>
public sealed record AdjustmentThresholdValidator(List<IAdjustmentThresholdRule> Rules)
{
    public List<string> Validate(AdjustmentThreshold item)
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

    public void RequireValid(AdjustmentThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentThreshold> items)
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

/// <summary>Functions of adjustment threshold rules.</summary>
public static class AdjustmentThresholdRules
{
    public static AdjustmentThresholdValidator DefaultAdjustmentThresholdValidator()
    {
        return new AdjustmentThresholdValidator(
            new List<IAdjustmentThresholdRule>
            {
                new AdjustmentThresholdSourceLengthRule(3600),
                new AdjustmentThresholdCodeLengthRule(16),
                new AdjustmentThresholdRequiredJobCodeRule("adjustment threshold needs this field"),
            });
    }
}
