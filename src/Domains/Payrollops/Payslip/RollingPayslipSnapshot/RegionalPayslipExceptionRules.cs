namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional payslip exception must pass. Check returns its problems, or none.</summary>
public interface IRegionalPayslipExceptionRule
{
    List<string> Check(RegionalPayslipException item);
}

public sealed record RegionalPayslipExceptionAllowedTitleRule(List<string> Allowed) : IRegionalPayslipExceptionRule
{
    public List<string> Check(RegionalPayslipException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayslipExceptionRequiredTitleRule(string Message) : IRegionalPayslipExceptionRule
{
    public List<string> Check(RegionalPayslipException item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayslipExceptionLabelLengthRule(int MaxLength) : IRegionalPayslipExceptionRule
{
    public List<string> Check(RegionalPayslipException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayslipExceptionExpiresAtRangeRule(int Min, int Max) : IRegionalPayslipExceptionRule
{
    public List<string> Check(RegionalPayslipException item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalPayslipExceptionCodePrefixRule(string Prefix) : IRegionalPayslipExceptionRule
{
    public List<string> Check(RegionalPayslipException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayslipExceptionRequiredCodeRule(string Message) : IRegionalPayslipExceptionRule
{
    public List<string> Check(RegionalPayslipException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional payslip exception.</summary>
public sealed record RegionalPayslipExceptionValidator(List<IRegionalPayslipExceptionRule> Rules)
{
    public List<string> Validate(RegionalPayslipException item)
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

    public void RequireValid(RegionalPayslipException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPayslipException> items)
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

/// <summary>Functions of regional payslip exception rules.</summary>
public static class RegionalPayslipExceptionRules
{
    public static RegionalPayslipExceptionValidator DefaultRegionalPayslipExceptionValidator()
    {
        return new RegionalPayslipExceptionValidator(
            new List<IRegionalPayslipExceptionRule>
            {
                new RegionalPayslipExceptionAllowedTitleRule(new List<string>()),
                new RegionalPayslipExceptionRequiredTitleRule("regional payslip exception needs this field"),
                new RegionalPayslipExceptionLabelLengthRule(3600),
                new RegionalPayslipExceptionExpiresAtRangeRule(120, 1000),
                new RegionalPayslipExceptionCodePrefixRule("ACM"),
                new RegionalPayslipExceptionRequiredCodeRule("regional payslip exception needs an entry"),
            });
    }
}
