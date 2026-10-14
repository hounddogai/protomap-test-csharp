namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip entry must pass. Check returns its problems, or none.</summary>
public interface IPayslipEntryRule
{
    List<string> Check(PayslipEntry item);
}

public sealed record PayslipEntryRetriesRangeRule(int Min, int Max) : IPayslipEntryRule
{
    public List<string> Check(PayslipEntry item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipEntryCeilingRangeRule(double Min, double Max) : IPayslipEntryRule
{
    public List<string> Check(PayslipEntry item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipEntryRequiredLocationRule(string Message) : IPayslipEntryRule
{
    public List<string> Check(PayslipEntry item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipEntryCodePrefixRule(string Prefix) : IPayslipEntryRule
{
    public List<string> Check(PayslipEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipEntryCodeLengthRule(int MaxLength) : IPayslipEntryRule
{
    public List<string> Check(PayslipEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipEntryAllowedCodeRule(List<string> Allowed) : IPayslipEntryRule
{
    public List<string> Check(PayslipEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip entry.</summary>
public sealed record PayslipEntryValidator(List<IPayslipEntryRule> Rules)
{
    public List<string> Validate(PayslipEntry item)
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

    public void RequireValid(PayslipEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipEntry> items)
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

/// <summary>Functions of payslip entry rules.</summary>
public static class PayslipEntryRules
{
    public static PayslipEntryValidator DefaultPayslipEntryValidator()
    {
        return new PayslipEntryValidator(
            new List<IPayslipEntryRule>
            {
                new PayslipEntryRetriesRangeRule(16, 80),
                new PayslipEntryCeilingRangeRule(9.1, 2218.1),
                new PayslipEntryRequiredLocationRule("payslip entry needs an entry"),
                new PayslipEntryCodePrefixRule("Q"),
                new PayslipEntryCodeLengthRule(80),
                new PayslipEntryAllowedCodeRule(new List<string>()),
            });
    }
}
