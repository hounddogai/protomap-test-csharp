namespace Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>A rule a fallback asset return threshold must pass. Check returns its problems, or none.</summary>
public interface IFallbackAssetReturnThresholdRule
{
    List<string> Check(FallbackAssetReturnThreshold item);
}

public sealed record FallbackAssetReturnThresholdMaximumRangeRule(double Min, double Max) : IFallbackAssetReturnThresholdRule
{
    public List<string> Check(FallbackAssetReturnThreshold item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackAssetReturnThresholdJobCodeLengthRule(int MaxLength) : IFallbackAssetReturnThresholdRule
{
    public List<string> Check(FallbackAssetReturnThreshold item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnThresholdLabelLengthRule(int MaxLength) : IFallbackAssetReturnThresholdRule
{
    public List<string> Check(FallbackAssetReturnThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnThresholdCodeLengthRule(int MaxLength) : IFallbackAssetReturnThresholdRule
{
    public List<string> Check(FallbackAssetReturnThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback asset return threshold.</summary>
public sealed record FallbackAssetReturnThresholdValidator(List<IFallbackAssetReturnThresholdRule> Rules)
{
    public List<string> Validate(FallbackAssetReturnThreshold item)
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

    public void RequireValid(FallbackAssetReturnThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAssetReturnThreshold> items)
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

/// <summary>Functions of fallback asset return threshold rules.</summary>
public static class FallbackAssetReturnThresholdRules
{
    public static FallbackAssetReturnThresholdValidator DefaultFallbackAssetReturnThresholdValidator()
    {
        return new FallbackAssetReturnThresholdValidator(
            new List<IFallbackAssetReturnThresholdRule>
            {
                new FallbackAssetReturnThresholdMaximumRangeRule(7.8, 3973.1),
                new FallbackAssetReturnThresholdJobCodeLengthRule(255),
                new FallbackAssetReturnThresholdLabelLengthRule(80),
                new FallbackAssetReturnThresholdCodeLengthRule(16),
            });
    }
}
