namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback asset return draft must pass. Check returns its problems, or none.</summary>
public interface IFallbackAssetReturnDraftRule
{
    List<string> Check(FallbackAssetReturnDraft item);
}

public sealed record FallbackAssetReturnDraftCurrencyLengthRule(int MaxLength) : IFallbackAssetReturnDraftRule
{
    public List<string> Check(FallbackAssetReturnDraft item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnDraftRequiredTitleRule(string Message) : IFallbackAssetReturnDraftRule
{
    public List<string> Check(FallbackAssetReturnDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnDraftAllowedTitleRule(List<string> Allowed) : IFallbackAssetReturnDraftRule
{
    public List<string> Check(FallbackAssetReturnDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnDraftCodeLengthRule(int MaxLength) : IFallbackAssetReturnDraftRule
{
    public List<string> Check(FallbackAssetReturnDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnDraftTitleLengthRule(int MaxLength) : IFallbackAssetReturnDraftRule
{
    public List<string> Check(FallbackAssetReturnDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAssetReturnDraftDiscountRangeRule(double Min, double Max) : IFallbackAssetReturnDraftRule
{
    public List<string> Check(FallbackAssetReturnDraft item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback asset return draft.</summary>
public sealed record FallbackAssetReturnDraftValidator(List<IFallbackAssetReturnDraftRule> Rules)
{
    public List<string> Validate(FallbackAssetReturnDraft item)
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

    public void RequireValid(FallbackAssetReturnDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAssetReturnDraft> items)
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

/// <summary>Functions of fallback asset return draft rules.</summary>
public static class FallbackAssetReturnDraftRules
{
    public static FallbackAssetReturnDraftValidator DefaultFallbackAssetReturnDraftValidator()
    {
        return new FallbackAssetReturnDraftValidator(
            new List<IFallbackAssetReturnDraftRule>
            {
                new FallbackAssetReturnDraftCurrencyLengthRule(86400),
                new FallbackAssetReturnDraftRequiredTitleRule("fallback asset return draft needs a value"),
                new FallbackAssetReturnDraftAllowedTitleRule(new List<string>()),
                new FallbackAssetReturnDraftCodeLengthRule(120),
                new FallbackAssetReturnDraftTitleLengthRule(1000),
                new FallbackAssetReturnDraftDiscountRangeRule(5.7, 1208.9),
            });
    }
}
