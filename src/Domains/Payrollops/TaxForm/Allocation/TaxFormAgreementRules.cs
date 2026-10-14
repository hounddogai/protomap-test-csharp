namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>A rule a tax form agreement must pass. Check returns its problems, or none.</summary>
public interface ITaxFormAgreementRule
{
    List<string> Check(TaxFormAgreement item);
}

public sealed record TaxFormAgreementRequiredTitleRule(string Message) : ITaxFormAgreementRule
{
    public List<string> Check(TaxFormAgreement item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAgreementCodeLengthRule(int MaxLength) : ITaxFormAgreementRule
{
    public List<string> Check(TaxFormAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAgreementTitleLengthRule(int MaxLength) : ITaxFormAgreementRule
{
    public List<string> Check(TaxFormAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAgreementCodePrefixRule(string Prefix) : ITaxFormAgreementRule
{
    public List<string> Check(TaxFormAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form agreement.</summary>
public sealed record TaxFormAgreementValidator(List<ITaxFormAgreementRule> Rules)
{
    public List<string> Validate(TaxFormAgreement item)
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

    public void RequireValid(TaxFormAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormAgreement> items)
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

/// <summary>Functions of tax form agreement rules.</summary>
public static class TaxFormAgreementRules
{
    public static TaxFormAgreementValidator DefaultTaxFormAgreementValidator()
    {
        return new TaxFormAgreementValidator(
            new List<ITaxFormAgreementRule>
            {
                new TaxFormAgreementRequiredTitleRule("tax form agreement needs a value"),
                new TaxFormAgreementCodeLengthRule(3600),
                new TaxFormAgreementTitleLengthRule(80),
                new TaxFormAgreementCodePrefixRule("OPS"),
            });
    }
}
