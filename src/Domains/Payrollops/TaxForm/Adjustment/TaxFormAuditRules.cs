namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form audit must pass. Check returns its problems, or none.</summary>
public interface ITaxFormAuditRule
{
    List<string> Check(TaxFormAudit item);
}

public sealed record TaxFormAuditGradeLengthRule(int MaxLength) : ITaxFormAuditRule
{
    public List<string> Check(TaxFormAudit item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAuditRequiredTitleRule(string Message) : ITaxFormAuditRule
{
    public List<string> Check(TaxFormAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAuditAllowedLocationRule(List<string> Allowed) : ITaxFormAuditRule
{
    public List<string> Check(TaxFormAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAuditCodePrefixRule(string Prefix) : ITaxFormAuditRule
{
    public List<string> Check(TaxFormAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form audit.</summary>
public sealed record TaxFormAuditValidator(List<ITaxFormAuditRule> Rules)
{
    public List<string> Validate(TaxFormAudit item)
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

    public void RequireValid(TaxFormAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormAudit> items)
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

/// <summary>Functions of tax form audit rules.</summary>
public static class TaxFormAuditRules
{
    public static TaxFormAuditValidator DefaultTaxFormAuditValidator()
    {
        return new TaxFormAuditValidator(
            new List<ITaxFormAuditRule>
            {
                new TaxFormAuditGradeLengthRule(255),
                new TaxFormAuditRequiredTitleRule("tax form audit needs this field"),
                new TaxFormAuditAllowedLocationRule(new List<string> { "weighted", "seasonal" }),
                new TaxFormAuditCodePrefixRule("X"),
            });
    }
}
