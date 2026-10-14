namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment ledger must pass. Check returns its problems, or none.</summary>
public interface IAssignmentLedgerRule
{
    List<string> Check(AssignmentLedger item);
}

public sealed record AssignmentLedgerLocationLengthRule(int MaxLength) : IAssignmentLedgerRule
{
    public List<string> Check(AssignmentLedger item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentLedgerCodeLengthRule(int MaxLength) : IAssignmentLedgerRule
{
    public List<string> Check(AssignmentLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentLedgerSalaryBandLengthRule(int MaxLength) : IAssignmentLedgerRule
{
    public List<string> Check(AssignmentLedger item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentLedgerLabelLengthRule(int MaxLength) : IAssignmentLedgerRule
{
    public List<string> Check(AssignmentLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentLedgerAllowedRegionRule(List<string> Allowed) : IAssignmentLedgerRule
{
    public List<string> Check(AssignmentLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentLedgerRequiredLocationRule(string Message) : IAssignmentLedgerRule
{
    public List<string> Check(AssignmentLedger item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment ledger.</summary>
public sealed record AssignmentLedgerValidator(List<IAssignmentLedgerRule> Rules)
{
    public List<string> Validate(AssignmentLedger item)
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

    public void RequireValid(AssignmentLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentLedger> items)
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

/// <summary>Functions of assignment ledger rules.</summary>
public static class AssignmentLedgerRules
{
    public static AssignmentLedgerValidator DefaultAssignmentLedgerValidator()
    {
        return new AssignmentLedgerValidator(
            new List<IAssignmentLedgerRule>
            {
                new AssignmentLedgerLocationLengthRule(120),
                new AssignmentLedgerCodeLengthRule(1000),
                new AssignmentLedgerSalaryBandLengthRule(120),
                new AssignmentLedgerLabelLengthRule(120),
                new AssignmentLedgerAllowedRegionRule(new List<string>()),
                new AssignmentLedgerRequiredLocationRule("assignment ledger needs a value"),
            });
    }
}
