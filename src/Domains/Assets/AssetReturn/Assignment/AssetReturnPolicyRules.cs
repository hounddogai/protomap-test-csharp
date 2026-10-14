namespace Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>A rule a asset return policy must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnPolicyRule
{
    List<string> Check(AssetReturnPolicy item);
}

public sealed record AssetReturnPolicyCodePrefixRule(string Prefix) : IAssetReturnPolicyRule
{
    public List<string> Check(AssetReturnPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPolicyRequiredGradeRule(string Message) : IAssetReturnPolicyRule
{
    public List<string> Check(AssetReturnPolicy item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPolicyThresholdRangeRule(double Min, double Max) : IAssetReturnPolicyRule
{
    public List<string> Check(AssetReturnPolicy item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPolicyRequiredLabelRule(string Message) : IAssetReturnPolicyRule
{
    public List<string> Check(AssetReturnPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPolicyGradeLengthRule(int MaxLength) : IAssetReturnPolicyRule
{
    public List<string> Check(AssetReturnPolicy item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return policy.</summary>
public sealed record AssetReturnPolicyValidator(List<IAssetReturnPolicyRule> Rules)
{
    public List<string> Validate(AssetReturnPolicy item)
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

    public void RequireValid(AssetReturnPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnPolicy> items)
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

/// <summary>Functions of asset return policy rules.</summary>
public static class AssetReturnPolicyRules
{
    public static AssetReturnPolicyValidator DefaultAssetReturnPolicyValidator()
    {
        return new AssetReturnPolicyValidator(
            new List<IAssetReturnPolicyRule>
            {
                new AssetReturnPolicyCodePrefixRule("X"),
                new AssetReturnPolicyRequiredGradeRule("asset return policy needs an entry"),
                new AssetReturnPolicyThresholdRangeRule(6.4, 1950.9),
                new AssetReturnPolicyRequiredLabelRule("asset return policy needs a value"),
                new AssetReturnPolicyGradeLengthRule(120),
            });
    }
}
