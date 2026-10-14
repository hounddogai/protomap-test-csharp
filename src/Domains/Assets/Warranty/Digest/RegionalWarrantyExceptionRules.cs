namespace Acme.Workforce.Assets.Warranty.Digest;

/// <summary>A rule a regional warranty exception must pass. Check returns its problems, or none.</summary>
public interface IRegionalWarrantyExceptionRule
{
    List<string> Check(RegionalWarrantyException item);
}

public sealed record RegionalWarrantyExceptionCodeLengthRule(int MaxLength) : IRegionalWarrantyExceptionRule
{
    public List<string> Check(RegionalWarrantyException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalWarrantyExceptionNameLengthRule(int MaxLength) : IRegionalWarrantyExceptionRule
{
    public List<string> Check(RegionalWarrantyException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalWarrantyExceptionAmountRangeRule(double Min, double Max) : IRegionalWarrantyExceptionRule
{
    public List<string> Check(RegionalWarrantyException item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalWarrantyExceptionCurrencyLengthRule(int MaxLength) : IRegionalWarrantyExceptionRule
{
    public List<string> Check(RegionalWarrantyException item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalWarrantyExceptionMarginRangeRule(double Min, double Max) : IRegionalWarrantyExceptionRule
{
    public List<string> Check(RegionalWarrantyException item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional warranty exception.</summary>
public sealed record RegionalWarrantyExceptionValidator(List<IRegionalWarrantyExceptionRule> Rules)
{
    public List<string> Validate(RegionalWarrantyException item)
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

    public void RequireValid(RegionalWarrantyException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalWarrantyException> items)
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

/// <summary>Functions of regional warranty exception rules.</summary>
public static class RegionalWarrantyExceptionRules
{
    public static RegionalWarrantyExceptionValidator DefaultRegionalWarrantyExceptionValidator()
    {
        return new RegionalWarrantyExceptionValidator(
            new List<IRegionalWarrantyExceptionRule>
            {
                new RegionalWarrantyExceptionCodeLengthRule(1000),
                new RegionalWarrantyExceptionNameLengthRule(255),
                new RegionalWarrantyExceptionAmountRangeRule(3.8, 4545.6),
                new RegionalWarrantyExceptionCurrencyLengthRule(32),
                new RegionalWarrantyExceptionMarginRangeRule(7.0, 3238.0),
            });
    }
}
