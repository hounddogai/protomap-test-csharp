namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form entry must pass. Check returns its problems, or none.</summary>
public interface ITaxFormEntryRule
{
    List<string> Check(TaxFormEntry item);
}

public sealed record TaxFormEntryRequiredCodeRule(string Message) : ITaxFormEntryRule
{
    public List<string> Check(TaxFormEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormEntryAllowedLabelRule(List<string> Allowed) : ITaxFormEntryRule
{
    public List<string> Check(TaxFormEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormEntryRequiredRegionRule(string Message) : ITaxFormEntryRule
{
    public List<string> Check(TaxFormEntry item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form entry.</summary>
public sealed record TaxFormEntryValidator(List<ITaxFormEntryRule> Rules)
{
    public List<string> Validate(TaxFormEntry item)
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

    public void RequireValid(TaxFormEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormEntry> items)
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

/// <summary>Functions of tax form entry rules.</summary>
public static class TaxFormEntryRules
{
    public static TaxFormEntryValidator DefaultTaxFormEntryValidator()
    {
        return new TaxFormEntryValidator(
            new List<ITaxFormEntryRule>
            {
                new TaxFormEntryRequiredCodeRule("tax form entry needs an entry"),
                new TaxFormEntryAllowedLabelRule(new List<string> { "seasonal" }),
                new TaxFormEntryRequiredRegionRule("tax form entry needs this field"),
            });
    }
}
