namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>A rule a tax form override must pass. Check returns its problems, or none.</summary>
public interface ITaxFormOverrideRule
{
    List<string> Check(TaxFormOverride item);
}

public sealed record TaxFormOverrideCodePrefixRule(string Prefix) : ITaxFormOverrideRule
{
    public List<string> Check(TaxFormOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormOverrideExpiresAtRangeRule(int Min, int Max) : ITaxFormOverrideRule
{
    public List<string> Check(TaxFormOverride item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormOverrideCodeLengthRule(int MaxLength) : ITaxFormOverrideRule
{
    public List<string> Check(TaxFormOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormOverrideRequiredSalaryBandRule(string Message) : ITaxFormOverrideRule
{
    public List<string> Check(TaxFormOverride item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormOverrideMinimumRangeRule(double Min, double Max) : ITaxFormOverrideRule
{
    public List<string> Check(TaxFormOverride item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form override.</summary>
public sealed record TaxFormOverrideValidator(List<ITaxFormOverrideRule> Rules)
{
    public List<string> Validate(TaxFormOverride item)
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

    public void RequireValid(TaxFormOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormOverride> items)
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

/// <summary>Functions of tax form override rules.</summary>
public static class TaxFormOverrideRules
{
    public static TaxFormOverrideValidator DefaultTaxFormOverrideValidator()
    {
        return new TaxFormOverrideValidator(
            new List<ITaxFormOverrideRule>
            {
                new TaxFormOverrideCodePrefixRule("OPS"),
                new TaxFormOverrideExpiresAtRangeRule(16, 255),
                new TaxFormOverrideCodeLengthRule(80),
                new TaxFormOverrideRequiredSalaryBandRule("tax form override needs an entry"),
                new TaxFormOverrideMinimumRangeRule(3.5, 506.5),
            });
    }
}
