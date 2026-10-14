namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling payslip assignment must pass. Check returns its problems, or none.</summary>
public interface IRollingPayslipAssignmentRule
{
    List<string> Check(RollingPayslipAssignment item);
}

public sealed record RollingPayslipAssignmentCodePrefixRule(string Prefix) : IRollingPayslipAssignmentRule
{
    public List<string> Check(RollingPayslipAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayslipAssignmentTeamLengthRule(int MaxLength) : IRollingPayslipAssignmentRule
{
    public List<string> Check(RollingPayslipAssignment item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayslipAssignmentAllowedCurrencyRule(List<string> Allowed) : IRollingPayslipAssignmentRule
{
    public List<string> Check(RollingPayslipAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayslipAssignmentRequiredCurrencyRule(string Message) : IRollingPayslipAssignmentRule
{
    public List<string> Check(RollingPayslipAssignment item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPayslipAssignmentLabelLengthRule(int MaxLength) : IRollingPayslipAssignmentRule
{
    public List<string> Check(RollingPayslipAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling payslip assignment.</summary>
public sealed record RollingPayslipAssignmentValidator(List<IRollingPayslipAssignmentRule> Rules)
{
    public List<string> Validate(RollingPayslipAssignment item)
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

    public void RequireValid(RollingPayslipAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPayslipAssignment> items)
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

/// <summary>Functions of rolling payslip assignment rules.</summary>
public static class RollingPayslipAssignmentRules
{
    public static RollingPayslipAssignmentValidator DefaultRollingPayslipAssignmentValidator()
    {
        return new RollingPayslipAssignmentValidator(
            new List<IRollingPayslipAssignmentRule>
            {
                new RollingPayslipAssignmentCodePrefixRule("X"),
                new RollingPayslipAssignmentTeamLengthRule(80),
                new RollingPayslipAssignmentAllowedCurrencyRule(new List<string>()),
                new RollingPayslipAssignmentRequiredCurrencyRule("rolling payslip assignment needs a value"),
                new RollingPayslipAssignmentLabelLengthRule(3600),
            });
    }
}
