namespace Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>A rule a tiered tax form bundle must pass. Check returns its problems, or none.</summary>
public interface ITieredTaxFormBundleRule
{
    List<string> Check(TieredTaxFormBundle item);
}

public sealed record TieredTaxFormBundleThresholdRangeRule(double Min, double Max) : ITieredTaxFormBundleRule
{
    public List<string> Check(TieredTaxFormBundle item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredTaxFormBundleRequiredNameRule(string Message) : ITieredTaxFormBundleRule
{
    public List<string> Check(TieredTaxFormBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredTaxFormBundleNameLengthRule(int MaxLength) : ITieredTaxFormBundleRule
{
    public List<string> Check(TieredTaxFormBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTaxFormBundleCodePrefixRule(string Prefix) : ITieredTaxFormBundleRule
{
    public List<string> Check(TieredTaxFormBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTaxFormBundleRequiredCodeRule(string Message) : ITieredTaxFormBundleRule
{
    public List<string> Check(TieredTaxFormBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredTaxFormBundleAmountRangeRule(double Min, double Max) : ITieredTaxFormBundleRule
{
    public List<string> Check(TieredTaxFormBundle item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered tax form bundle.</summary>
public sealed record TieredTaxFormBundleValidator(List<ITieredTaxFormBundleRule> Rules)
{
    public List<string> Validate(TieredTaxFormBundle item)
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

    public void RequireValid(TieredTaxFormBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredTaxFormBundle> items)
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

/// <summary>Functions of tiered tax form bundle rules.</summary>
public static class TieredTaxFormBundleRules
{
    public static TieredTaxFormBundleValidator DefaultTieredTaxFormBundleValidator()
    {
        return new TieredTaxFormBundleValidator(
            new List<ITieredTaxFormBundleRule>
            {
                new TieredTaxFormBundleThresholdRangeRule(3.6, 2077.1),
                new TieredTaxFormBundleRequiredNameRule("tiered tax form bundle needs an entry"),
                new TieredTaxFormBundleNameLengthRule(64),
                new TieredTaxFormBundleCodePrefixRule("REF"),
                new TieredTaxFormBundleRequiredCodeRule("tiered tax form bundle needs this field"),
                new TieredTaxFormBundleAmountRangeRule(7.5, 1742.7),
            });
    }
}
