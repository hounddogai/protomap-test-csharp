namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal warranty digest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalWarrantyDigestRule
{
    List<string> Check(SeasonalWarrantyDigest item);
}

public sealed record SeasonalWarrantyDigestTitleLengthRule(int MaxLength) : ISeasonalWarrantyDigestRule
{
    public List<string> Check(SeasonalWarrantyDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyDigestVisibleGuardRule(double Limit) : ISeasonalWarrantyDigestRule
{
    public List<string> Check(SeasonalWarrantyDigest item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyDigestCodeLengthRule(int MaxLength) : ISeasonalWarrantyDigestRule
{
    public List<string> Check(SeasonalWarrantyDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyDigestAmountRangeRule(double Min, double Max) : ISeasonalWarrantyDigestRule
{
    public List<string> Check(SeasonalWarrantyDigest item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyDigestRequiredTitleRule(string Message) : ISeasonalWarrantyDigestRule
{
    public List<string> Check(SeasonalWarrantyDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyDigestAllowedJobCodeRule(List<string> Allowed) : ISeasonalWarrantyDigestRule
{
    public List<string> Check(SeasonalWarrantyDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal warranty digest.</summary>
public sealed record SeasonalWarrantyDigestValidator(List<ISeasonalWarrantyDigestRule> Rules)
{
    public List<string> Validate(SeasonalWarrantyDigest item)
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

    public void RequireValid(SeasonalWarrantyDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalWarrantyDigest> items)
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

/// <summary>Functions of seasonal warranty digest rules.</summary>
public static class SeasonalWarrantyDigestRules
{
    public static SeasonalWarrantyDigestValidator DefaultSeasonalWarrantyDigestValidator()
    {
        return new SeasonalWarrantyDigestValidator(
            new List<ISeasonalWarrantyDigestRule>
            {
                new SeasonalWarrantyDigestTitleLengthRule(64),
                new SeasonalWarrantyDigestVisibleGuardRule(3.6),
                new SeasonalWarrantyDigestCodeLengthRule(80),
                new SeasonalWarrantyDigestAmountRangeRule(0.5, 3953.9),
                new SeasonalWarrantyDigestRequiredTitleRule("seasonal warranty digest needs this field"),
                new SeasonalWarrantyDigestAllowedJobCodeRule(new List<string> { "weighted" }),
            });
    }
}
