namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary payslip snapshot must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPayslipSnapshotRule
{
    List<string> Check(PrimaryPayslipSnapshot item);
}

public sealed record PrimaryPayslipSnapshotAllowedLabelRule(List<string> Allowed) : IPrimaryPayslipSnapshotRule
{
    public List<string> Check(PrimaryPayslipSnapshot item)
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

public sealed record PrimaryPayslipSnapshotRequiredLocationRule(string Message) : IPrimaryPayslipSnapshotRule
{
    public List<string> Check(PrimaryPayslipSnapshot item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayslipSnapshotRequiredTeamRule(string Message) : IPrimaryPayslipSnapshotRule
{
    public List<string> Check(PrimaryPayslipSnapshot item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayslipSnapshotRequiredCodeRule(string Message) : IPrimaryPayslipSnapshotRule
{
    public List<string> Check(PrimaryPayslipSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayslipSnapshotLocationLengthRule(int MaxLength) : IPrimaryPayslipSnapshotRule
{
    public List<string> Check(PrimaryPayslipSnapshot item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPayslipSnapshotRequiredLabelRule(string Message) : IPrimaryPayslipSnapshotRule
{
    public List<string> Check(PrimaryPayslipSnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary payslip snapshot.</summary>
public sealed record PrimaryPayslipSnapshotValidator(List<IPrimaryPayslipSnapshotRule> Rules)
{
    public List<string> Validate(PrimaryPayslipSnapshot item)
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

    public void RequireValid(PrimaryPayslipSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPayslipSnapshot> items)
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

/// <summary>Functions of primary payslip snapshot rules.</summary>
public static class PrimaryPayslipSnapshotRules
{
    public static PrimaryPayslipSnapshotValidator DefaultPrimaryPayslipSnapshotValidator()
    {
        return new PrimaryPayslipSnapshotValidator(
            new List<IPrimaryPayslipSnapshotRule>
            {
                new PrimaryPayslipSnapshotAllowedLabelRule(new List<string> { "adaptive", "adaptive" }),
                new PrimaryPayslipSnapshotRequiredLocationRule("primary payslip snapshot needs an entry"),
                new PrimaryPayslipSnapshotRequiredTeamRule("primary payslip snapshot needs this field"),
                new PrimaryPayslipSnapshotRequiredCodeRule("primary payslip snapshot needs a value"),
                new PrimaryPayslipSnapshotLocationLengthRule(120),
                new PrimaryPayslipSnapshotRequiredLabelRule("primary payslip snapshot needs an entry"),
            });
    }
}
