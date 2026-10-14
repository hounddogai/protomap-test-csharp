namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted recognition threshold must pass. Check returns its problems, or none.</summary>
public interface IWeightedRecognitionThresholdRule
{
    List<string> Check(WeightedRecognitionThreshold item);
}

public sealed record WeightedRecognitionThresholdRequiredBadgeRule(string Message) : IWeightedRecognitionThresholdRule
{
    public List<string> Check(WeightedRecognitionThreshold item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRecognitionThresholdAllowedCostCenterRule(List<string> Allowed) : IWeightedRecognitionThresholdRule
{
    public List<string> Check(WeightedRecognitionThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRecognitionThresholdCodeLengthRule(int MaxLength) : IWeightedRecognitionThresholdRule
{
    public List<string> Check(WeightedRecognitionThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRecognitionThresholdRequiredCodeRule(string Message) : IWeightedRecognitionThresholdRule
{
    public List<string> Check(WeightedRecognitionThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRecognitionThresholdBadgeLengthRule(int MaxLength) : IWeightedRecognitionThresholdRule
{
    public List<string> Check(WeightedRecognitionThreshold item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted recognition threshold.</summary>
public sealed record WeightedRecognitionThresholdValidator(List<IWeightedRecognitionThresholdRule> Rules)
{
    public List<string> Validate(WeightedRecognitionThreshold item)
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

    public void RequireValid(WeightedRecognitionThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRecognitionThreshold> items)
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

/// <summary>Functions of weighted recognition threshold rules.</summary>
public static class WeightedRecognitionThresholdRules
{
    public static WeightedRecognitionThresholdValidator DefaultWeightedRecognitionThresholdValidator()
    {
        return new WeightedRecognitionThresholdValidator(
            new List<IWeightedRecognitionThresholdRule>
            {
                new WeightedRecognitionThresholdRequiredBadgeRule("weighted recognition threshold needs a value"),
                new WeightedRecognitionThresholdAllowedCostCenterRule(
                    new List<string> { "seasonal", "tiered", "fallback" }),
                new WeightedRecognitionThresholdCodeLengthRule(1000),
                new WeightedRecognitionThresholdRequiredCodeRule("weighted recognition threshold needs an entry"),
                new WeightedRecognitionThresholdBadgeLengthRule(16),
            });
    }
}
