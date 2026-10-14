namespace Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>A rule a manual asset return baseline must pass. Check returns its problems, or none.</summary>
public interface IManualAssetReturnBaselineRule
{
    List<string> Check(ManualAssetReturnBaseline item);
}

public sealed record ManualAssetReturnBaselineCostRangeRule(double Min, double Max) : IManualAssetReturnBaselineRule
{
    public List<string> Check(ManualAssetReturnBaseline item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualAssetReturnBaselineCodeLengthRule(int MaxLength) : IManualAssetReturnBaselineRule
{
    public List<string> Check(ManualAssetReturnBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnBaselineRequiredNameRule(string Message) : IManualAssetReturnBaselineRule
{
    public List<string> Check(ManualAssetReturnBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnBaselineCreatedAtRangeRule(int Min, int Max) : IManualAssetReturnBaselineRule
{
    public List<string> Check(ManualAssetReturnBaseline item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnBaselineOwnerIdLengthRule(int MaxLength) : IManualAssetReturnBaselineRule
{
    public List<string> Check(ManualAssetReturnBaseline item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssetReturnBaselineCodePrefixRule(string Prefix) : IManualAssetReturnBaselineRule
{
    public List<string> Check(ManualAssetReturnBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual asset return baseline.</summary>
public sealed record ManualAssetReturnBaselineValidator(List<IManualAssetReturnBaselineRule> Rules)
{
    public List<string> Validate(ManualAssetReturnBaseline item)
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

    public void RequireValid(ManualAssetReturnBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAssetReturnBaseline> items)
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

/// <summary>Functions of manual asset return baseline rules.</summary>
public static class ManualAssetReturnBaselineRules
{
    public static ManualAssetReturnBaselineValidator DefaultManualAssetReturnBaselineValidator()
    {
        return new ManualAssetReturnBaselineValidator(
            new List<IManualAssetReturnBaselineRule>
            {
                new ManualAssetReturnBaselineCostRangeRule(1.6, 1275.6),
                new ManualAssetReturnBaselineCodeLengthRule(64),
                new ManualAssetReturnBaselineRequiredNameRule("manual asset return baseline needs a value"),
                new ManualAssetReturnBaselineCreatedAtRangeRule(16, 86400),
                new ManualAssetReturnBaselineOwnerIdLengthRule(255),
                new ManualAssetReturnBaselineCodePrefixRule("OPS"),
            });
    }
}
