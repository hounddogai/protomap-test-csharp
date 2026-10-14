namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>A rule a seasonal tax form tier must pass. Check returns its problems, or none.</summary>
public interface ISeasonalTaxFormTierRule
{
    List<string> Check(SeasonalTaxFormTier item);
}

public sealed record SeasonalTaxFormTierCurrencyLengthRule(int MaxLength) : ISeasonalTaxFormTierRule
{
    public List<string> Check(SeasonalTaxFormTier item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormTierRequiredTitleRule(string Message) : ISeasonalTaxFormTierRule
{
    public List<string> Check(SeasonalTaxFormTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormTierRequiredCurrencyRule(string Message) : ISeasonalTaxFormTierRule
{
    public List<string> Check(SeasonalTaxFormTier item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormTierRequiredLabelRule(string Message) : ISeasonalTaxFormTierRule
{
    public List<string> Check(SeasonalTaxFormTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal tax form tier.</summary>
public sealed record SeasonalTaxFormTierValidator(List<ISeasonalTaxFormTierRule> Rules)
{
    public List<string> Validate(SeasonalTaxFormTier item)
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

    public void RequireValid(SeasonalTaxFormTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalTaxFormTier> items)
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

/// <summary>Functions of seasonal tax form tier rules.</summary>
public static class SeasonalTaxFormTierRules
{
    public static SeasonalTaxFormTierValidator DefaultSeasonalTaxFormTierValidator()
    {
        return new SeasonalTaxFormTierValidator(
            new List<ISeasonalTaxFormTierRule>
            {
                new SeasonalTaxFormTierCurrencyLengthRule(86400),
                new SeasonalTaxFormTierRequiredTitleRule("seasonal tax form tier needs a value"),
                new SeasonalTaxFormTierRequiredCurrencyRule("seasonal tax form tier needs this field"),
                new SeasonalTaxFormTierRequiredLabelRule("seasonal tax form tier needs this field"),
            });
    }
}
