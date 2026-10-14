namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form exception must pass. Check returns its problems, or none.</summary>
public interface ITaxFormExceptionRule
{
    List<string> Check(TaxFormException item);
}

public sealed record TaxFormExceptionRequiredCurrencyRule(string Message) : ITaxFormExceptionRule
{
    public List<string> Check(TaxFormException item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormExceptionCurrencyLengthRule(int MaxLength) : ITaxFormExceptionRule
{
    public List<string> Check(TaxFormException item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormExceptionRequiredCategoryRule(string Message) : ITaxFormExceptionRule
{
    public List<string> Check(TaxFormException item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormExceptionCodeLengthRule(int MaxLength) : ITaxFormExceptionRule
{
    public List<string> Check(TaxFormException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormExceptionAllowedCategoryRule(List<string> Allowed) : ITaxFormExceptionRule
{
    public List<string> Check(TaxFormException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form exception.</summary>
public sealed record TaxFormExceptionValidator(List<ITaxFormExceptionRule> Rules)
{
    public List<string> Validate(TaxFormException item)
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

    public void RequireValid(TaxFormException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormException> items)
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

/// <summary>Functions of tax form exception rules.</summary>
public static class TaxFormExceptionRules
{
    public static TaxFormExceptionValidator DefaultTaxFormExceptionValidator()
    {
        return new TaxFormExceptionValidator(
            new List<ITaxFormExceptionRule>
            {
                new TaxFormExceptionRequiredCurrencyRule("tax form exception needs this field"),
                new TaxFormExceptionCurrencyLengthRule(16),
                new TaxFormExceptionRequiredCategoryRule("tax form exception needs an entry"),
                new TaxFormExceptionCodeLengthRule(1000),
                new TaxFormExceptionAllowedCategoryRule(new List<string>()),
            });
    }
}
