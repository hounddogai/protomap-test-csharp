namespace Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>A rule a tax form mapping must pass. Check returns its problems, or none.</summary>
public interface ITaxFormMappingRule
{
    List<string> Check(TaxFormMapping item);
}

public sealed record TaxFormMappingRequiredCodeRule(string Message) : ITaxFormMappingRule
{
    public List<string> Check(TaxFormMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMappingRequiredLabelRule(string Message) : ITaxFormMappingRule
{
    public List<string> Check(TaxFormMapping item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMappingCodeLengthRule(int MaxLength) : ITaxFormMappingRule
{
    public List<string> Check(TaxFormMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMappingQuantityRangeRule(int Min, int Max) : ITaxFormMappingRule
{
    public List<string> Check(TaxFormMapping item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormMappingCodePrefixRule(string Prefix) : ITaxFormMappingRule
{
    public List<string> Check(TaxFormMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form mapping.</summary>
public sealed record TaxFormMappingValidator(List<ITaxFormMappingRule> Rules)
{
    public List<string> Validate(TaxFormMapping item)
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

    public void RequireValid(TaxFormMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormMapping> items)
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

/// <summary>Functions of tax form mapping rules.</summary>
public static class TaxFormMappingRules
{
    public static TaxFormMappingValidator DefaultTaxFormMappingValidator()
    {
        return new TaxFormMappingValidator(
            new List<ITaxFormMappingRule>
            {
                new TaxFormMappingRequiredCodeRule("tax form mapping needs a value"),
                new TaxFormMappingRequiredLabelRule("tax form mapping needs an entry"),
                new TaxFormMappingCodeLengthRule(255),
                new TaxFormMappingQuantityRangeRule(32, 3600),
                new TaxFormMappingCodePrefixRule("X"),
            });
    }
}
