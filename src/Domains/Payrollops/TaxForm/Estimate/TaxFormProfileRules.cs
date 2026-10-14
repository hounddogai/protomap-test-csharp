namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form profile must pass. Check returns its problems, or none.</summary>
public interface ITaxFormProfileRule
{
    List<string> Check(TaxFormProfile item);
}

public sealed record TaxFormProfileCodeLengthRule(int MaxLength) : ITaxFormProfileRule
{
    public List<string> Check(TaxFormProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormProfileCodePrefixRule(string Prefix) : ITaxFormProfileRule
{
    public List<string> Check(TaxFormProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormProfileAllowedCodeRule(List<string> Allowed) : ITaxFormProfileRule
{
    public List<string> Check(TaxFormProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormProfileRequiredNameRule(string Message) : ITaxFormProfileRule
{
    public List<string> Check(TaxFormProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormProfileRequiredCodeRule(string Message) : ITaxFormProfileRule
{
    public List<string> Check(TaxFormProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormProfileRequiredCategoryRule(string Message) : ITaxFormProfileRule
{
    public List<string> Check(TaxFormProfile item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form profile.</summary>
public sealed record TaxFormProfileValidator(List<ITaxFormProfileRule> Rules)
{
    public List<string> Validate(TaxFormProfile item)
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

    public void RequireValid(TaxFormProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormProfile> items)
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

/// <summary>Functions of tax form profile rules.</summary>
public static class TaxFormProfileRules
{
    public static TaxFormProfileValidator DefaultTaxFormProfileValidator()
    {
        return new TaxFormProfileValidator(
            new List<ITaxFormProfileRule>
            {
                new TaxFormProfileCodeLengthRule(1000),
                new TaxFormProfileCodePrefixRule("ACM"),
                new TaxFormProfileAllowedCodeRule(new List<string> { "batched", "scheduled" }),
                new TaxFormProfileRequiredNameRule("tax form profile needs this field"),
                new TaxFormProfileRequiredCodeRule("tax form profile needs a value"),
                new TaxFormProfileRequiredCategoryRule("tax form profile needs a value"),
            });
    }
}
