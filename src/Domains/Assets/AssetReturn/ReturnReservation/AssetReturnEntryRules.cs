namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>A rule a asset return entry must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnEntryRule
{
    List<string> Check(AssetReturnEntry item);
}

public sealed record AssetReturnEntryTitleLengthRule(int MaxLength) : IAssetReturnEntryRule
{
    public List<string> Check(AssetReturnEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnEntryRequiredCurrencyRule(string Message) : IAssetReturnEntryRule
{
    public List<string> Check(AssetReturnEntry item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnEntryVersionRangeRule(int Min, int Max) : IAssetReturnEntryRule
{
    public List<string> Check(AssetReturnEntry item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnEntryRequiredTitleRule(string Message) : IAssetReturnEntryRule
{
    public List<string> Check(AssetReturnEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnEntryCodePrefixRule(string Prefix) : IAssetReturnEntryRule
{
    public List<string> Check(AssetReturnEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnEntryCodeLengthRule(int MaxLength) : IAssetReturnEntryRule
{
    public List<string> Check(AssetReturnEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return entry.</summary>
public sealed record AssetReturnEntryValidator(List<IAssetReturnEntryRule> Rules)
{
    public List<string> Validate(AssetReturnEntry item)
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

    public void RequireValid(AssetReturnEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnEntry> items)
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

/// <summary>Functions of asset return entry rules.</summary>
public static class AssetReturnEntryRules
{
    public static AssetReturnEntryValidator DefaultAssetReturnEntryValidator()
    {
        return new AssetReturnEntryValidator(
            new List<IAssetReturnEntryRule>
            {
                new AssetReturnEntryTitleLengthRule(255),
                new AssetReturnEntryRequiredCurrencyRule("asset return entry needs an entry"),
                new AssetReturnEntryVersionRangeRule(32, 86400),
                new AssetReturnEntryRequiredTitleRule("asset return entry needs a value"),
                new AssetReturnEntryCodePrefixRule("X"),
                new AssetReturnEntryCodeLengthRule(1000),
            });
    }
}
