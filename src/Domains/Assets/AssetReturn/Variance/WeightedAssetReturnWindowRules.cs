namespace Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>A rule a weighted asset return window must pass. Check returns its problems, or none.</summary>
public interface IWeightedAssetReturnWindowRule
{
    List<string> Check(WeightedAssetReturnWindow item);
}

public sealed record WeightedAssetReturnWindowRequiredCodeRule(string Message) : IWeightedAssetReturnWindowRule
{
    public List<string> Check(WeightedAssetReturnWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnWindowSourceLengthRule(int MaxLength) : IWeightedAssetReturnWindowRule
{
    public List<string> Check(WeightedAssetReturnWindow item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnWindowNameLengthRule(int MaxLength) : IWeightedAssetReturnWindowRule
{
    public List<string> Check(WeightedAssetReturnWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnWindowRequiredNameRule(string Message) : IWeightedAssetReturnWindowRule
{
    public List<string> Check(WeightedAssetReturnWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted asset return window.</summary>
public sealed record WeightedAssetReturnWindowValidator(List<IWeightedAssetReturnWindowRule> Rules)
{
    public List<string> Validate(WeightedAssetReturnWindow item)
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

    public void RequireValid(WeightedAssetReturnWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAssetReturnWindow> items)
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

/// <summary>Functions of weighted asset return window rules.</summary>
public static class WeightedAssetReturnWindowRules
{
    public static WeightedAssetReturnWindowValidator DefaultWeightedAssetReturnWindowValidator()
    {
        return new WeightedAssetReturnWindowValidator(
            new List<IWeightedAssetReturnWindowRule>
            {
                new WeightedAssetReturnWindowRequiredCodeRule("weighted asset return window needs this field"),
                new WeightedAssetReturnWindowSourceLengthRule(1000),
                new WeightedAssetReturnWindowNameLengthRule(80),
                new WeightedAssetReturnWindowRequiredNameRule("weighted asset return window needs this field"),
            });
    }
}
