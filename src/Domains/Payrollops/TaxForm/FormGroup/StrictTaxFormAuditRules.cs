namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict tax form audit must pass. Check returns its problems, or none.</summary>
public interface IStrictTaxFormAuditRule
{
    List<string> Check(StrictTaxFormAudit item);
}

public sealed record StrictTaxFormAuditCodeLengthRule(int MaxLength) : IStrictTaxFormAuditRule
{
    public List<string> Check(StrictTaxFormAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTaxFormAuditAllowedNameRule(List<string> Allowed) : IStrictTaxFormAuditRule
{
    public List<string> Check(StrictTaxFormAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record StrictTaxFormAuditNameLengthRule(int MaxLength) : IStrictTaxFormAuditRule
{
    public List<string> Check(StrictTaxFormAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTaxFormAuditNotesLengthRule(int MaxLength) : IStrictTaxFormAuditRule
{
    public List<string> Check(StrictTaxFormAudit item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict tax form audit.</summary>
public sealed record StrictTaxFormAuditValidator(List<IStrictTaxFormAuditRule> Rules)
{
    public List<string> Validate(StrictTaxFormAudit item)
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

    public void RequireValid(StrictTaxFormAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictTaxFormAudit> items)
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

/// <summary>Functions of strict tax form audit rules.</summary>
public static class StrictTaxFormAuditRules
{
    public static StrictTaxFormAuditValidator DefaultStrictTaxFormAuditValidator()
    {
        return new StrictTaxFormAuditValidator(
            new List<IStrictTaxFormAuditRule>
            {
                new StrictTaxFormAuditCodeLengthRule(80),
                new StrictTaxFormAuditAllowedNameRule(new List<string> { "pending", "legacy", "batched" }),
                new StrictTaxFormAuditNameLengthRule(64),
                new StrictTaxFormAuditNotesLengthRule(80),
            });
    }
}
