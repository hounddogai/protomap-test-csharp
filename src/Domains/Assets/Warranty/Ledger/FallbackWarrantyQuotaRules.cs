namespace Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>A rule a fallback warranty quota must pass. Check returns its problems, or none.</summary>
public interface IFallbackWarrantyQuotaRule
{
    List<string> Check(FallbackWarrantyQuota item);
}

public sealed record FallbackWarrantyQuotaTitleLengthRule(int MaxLength) : IFallbackWarrantyQuotaRule
{
    public List<string> Check(FallbackWarrantyQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackWarrantyQuotaRequiredSourceRule(string Message) : IFallbackWarrantyQuotaRule
{
    public List<string> Check(FallbackWarrantyQuota item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackWarrantyQuotaRequiredCodeRule(string Message) : IFallbackWarrantyQuotaRule
{
    public List<string> Check(FallbackWarrantyQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackWarrantyQuotaStartsAtRangeRule(int Min, int Max) : IFallbackWarrantyQuotaRule
{
    public List<string> Check(FallbackWarrantyQuota item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback warranty quota.</summary>
public sealed record FallbackWarrantyQuotaValidator(List<IFallbackWarrantyQuotaRule> Rules)
{
    public List<string> Validate(FallbackWarrantyQuota item)
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

    public void RequireValid(FallbackWarrantyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackWarrantyQuota> items)
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

/// <summary>Functions of fallback warranty quota rules.</summary>
public static class FallbackWarrantyQuotaRules
{
    public static FallbackWarrantyQuotaValidator DefaultFallbackWarrantyQuotaValidator()
    {
        return new FallbackWarrantyQuotaValidator(
            new List<IFallbackWarrantyQuotaRule>
            {
                new FallbackWarrantyQuotaTitleLengthRule(1000),
                new FallbackWarrantyQuotaRequiredSourceRule("fallback warranty quota needs an entry"),
                new FallbackWarrantyQuotaRequiredCodeRule("fallback warranty quota needs this field"),
                new FallbackWarrantyQuotaStartsAtRangeRule(16, 120),
            });
    }
}
