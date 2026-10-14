namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted policy ack bundle must pass. Check returns its problems, or none.</summary>
public interface IWeightedPolicyAckBundleRule
{
    List<string> Check(WeightedPolicyAckBundle item);
}

public sealed record WeightedPolicyAckBundleBadgeLengthRule(int MaxLength) : IWeightedPolicyAckBundleRule
{
    public List<string> Check(WeightedPolicyAckBundle item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckBundleRequiredCodeRule(string Message) : IWeightedPolicyAckBundleRule
{
    public List<string> Check(WeightedPolicyAckBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckBundleAllowedCodeRule(List<string> Allowed) : IWeightedPolicyAckBundleRule
{
    public List<string> Check(WeightedPolicyAckBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckBundleRatingRangeRule(double Min, double Max) : IWeightedPolicyAckBundleRule
{
    public List<string> Check(WeightedPolicyAckBundle item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedPolicyAckBundleRequiredBadgeRule(string Message) : IWeightedPolicyAckBundleRule
{
    public List<string> Check(WeightedPolicyAckBundle item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted policy ack bundle.</summary>
public sealed record WeightedPolicyAckBundleValidator(List<IWeightedPolicyAckBundleRule> Rules)
{
    public List<string> Validate(WeightedPolicyAckBundle item)
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

    public void RequireValid(WeightedPolicyAckBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPolicyAckBundle> items)
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

/// <summary>Functions of weighted policy ack bundle rules.</summary>
public static class WeightedPolicyAckBundleRules
{
    public static WeightedPolicyAckBundleValidator DefaultWeightedPolicyAckBundleValidator()
    {
        return new WeightedPolicyAckBundleValidator(
            new List<IWeightedPolicyAckBundleRule>
            {
                new WeightedPolicyAckBundleBadgeLengthRule(1000),
                new WeightedPolicyAckBundleRequiredCodeRule("weighted policy ack bundle needs an entry"),
                new WeightedPolicyAckBundleAllowedCodeRule(new List<string> { "fallback" }),
                new WeightedPolicyAckBundleRatingRangeRule(3.5, 844.0),
                new WeightedPolicyAckBundleRequiredBadgeRule("weighted policy ack bundle needs this field"),
            });
    }
}
