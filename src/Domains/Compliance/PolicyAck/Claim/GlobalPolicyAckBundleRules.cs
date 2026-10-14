namespace Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>A rule a global policy ack bundle must pass. Check returns its problems, or none.</summary>
public interface IGlobalPolicyAckBundleRule
{
    List<string> Check(GlobalPolicyAckBundle item);
}

public sealed record GlobalPolicyAckBundleCostCenterLengthRule(int MaxLength) : IGlobalPolicyAckBundleRule
{
    public List<string> Check(GlobalPolicyAckBundle item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyAckBundleCreatedAtRangeRule(int Min, int Max) : IGlobalPolicyAckBundleRule
{
    public List<string> Check(GlobalPolicyAckBundle item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyAckBundleUpdatedAtRangeRule(int Min, int Max) : IGlobalPolicyAckBundleRule
{
    public List<string> Check(GlobalPolicyAckBundle item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyAckBundleThresholdRangeRule(double Min, double Max) : IGlobalPolicyAckBundleRule
{
    public List<string> Check(GlobalPolicyAckBundle item)
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

/// <summary>Runs every rule of a global policy ack bundle.</summary>
public sealed record GlobalPolicyAckBundleValidator(List<IGlobalPolicyAckBundleRule> Rules)
{
    public List<string> Validate(GlobalPolicyAckBundle item)
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

    public void RequireValid(GlobalPolicyAckBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPolicyAckBundle> items)
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

/// <summary>Functions of global policy ack bundle rules.</summary>
public static class GlobalPolicyAckBundleRules
{
    public static GlobalPolicyAckBundleValidator DefaultGlobalPolicyAckBundleValidator()
    {
        return new GlobalPolicyAckBundleValidator(
            new List<IGlobalPolicyAckBundleRule>
            {
                new GlobalPolicyAckBundleCostCenterLengthRule(64),
                new GlobalPolicyAckBundleCreatedAtRangeRule(64, 3600),
                new GlobalPolicyAckBundleUpdatedAtRangeRule(16, 80),
                new GlobalPolicyAckBundleThresholdRangeRule(1.1, 1612.7),
            });
    }
}
