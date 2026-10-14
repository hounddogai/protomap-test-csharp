namespace Acme.Workforce.Assets.Warranty.Digest;

/// <summary>A rule a lenient warranty forecast must pass. Check returns its problems, or none.</summary>
public interface ILenientWarrantyForecastRule
{
    List<string> Check(LenientWarrantyForecast item);
}

public sealed record LenientWarrantyForecastRequiredCodeRule(string Message) : ILenientWarrantyForecastRule
{
    public List<string> Check(LenientWarrantyForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyForecastCodePrefixRule(string Prefix) : ILenientWarrantyForecastRule
{
    public List<string> Check(LenientWarrantyForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyForecastRequiredTitleRule(string Message) : ILenientWarrantyForecastRule
{
    public List<string> Check(LenientWarrantyForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient warranty forecast.</summary>
public sealed record LenientWarrantyForecastValidator(List<ILenientWarrantyForecastRule> Rules)
{
    public List<string> Validate(LenientWarrantyForecast item)
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

    public void RequireValid(LenientWarrantyForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientWarrantyForecast> items)
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

/// <summary>Functions of lenient warranty forecast rules.</summary>
public static class LenientWarrantyForecastRules
{
    public static LenientWarrantyForecastValidator DefaultLenientWarrantyForecastValidator()
    {
        return new LenientWarrantyForecastValidator(
            new List<ILenientWarrantyForecastRule>
            {
                new LenientWarrantyForecastRequiredCodeRule("lenient warranty forecast needs a value"),
                new LenientWarrantyForecastCodePrefixRule("Q"),
                new LenientWarrantyForecastRequiredTitleRule("lenient warranty forecast needs a value"),
            });
    }
}
