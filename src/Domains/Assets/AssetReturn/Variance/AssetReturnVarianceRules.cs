namespace Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>A rule a asset return variance must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnVarianceRule
{
    List<string> Check(AssetReturnVariance item);
}

public sealed record AssetReturnVarianceChannelLengthRule(int MaxLength) : IAssetReturnVarianceRule
{
    public List<string> Check(AssetReturnVariance item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnVarianceRequiredChannelRule(string Message) : IAssetReturnVarianceRule
{
    public List<string> Check(AssetReturnVariance item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnVarianceSequenceRangeRule(int Min, int Max) : IAssetReturnVarianceRule
{
    public List<string> Check(AssetReturnVariance item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnVarianceRequiredTimezoneRule(string Message) : IAssetReturnVarianceRule
{
    public List<string> Check(AssetReturnVariance item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnVarianceMarginRangeRule(double Min, double Max) : IAssetReturnVarianceRule
{
    public List<string> Check(AssetReturnVariance item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return variance.</summary>
public sealed record AssetReturnVarianceValidator(List<IAssetReturnVarianceRule> Rules)
{
    public List<string> Validate(AssetReturnVariance item)
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

    public void RequireValid(AssetReturnVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnVariance> items)
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

/// <summary>Functions of asset return variance rules.</summary>
public static class AssetReturnVarianceRules
{
    public static AssetReturnVarianceValidator DefaultAssetReturnVarianceValidator()
    {
        return new AssetReturnVarianceValidator(
            new List<IAssetReturnVarianceRule>
            {
                new AssetReturnVarianceChannelLengthRule(16),
                new AssetReturnVarianceRequiredChannelRule("asset return variance needs this field"),
                new AssetReturnVarianceSequenceRangeRule(16, 32),
                new AssetReturnVarianceRequiredTimezoneRule("asset return variance needs an entry"),
                new AssetReturnVarianceMarginRangeRule(9.3, 262.3),
            });
    }
}
