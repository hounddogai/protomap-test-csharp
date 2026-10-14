namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip notice must pass. Check returns its problems, or none.</summary>
public interface IPayslipNoticeRule
{
    List<string> Check(PayslipNotice item);
}

public sealed record PayslipNoticeRequiredLabelRule(string Message) : IPayslipNoticeRule
{
    public List<string> Check(PayslipNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipNoticeAllowedDescriptionRule(List<string> Allowed) : IPayslipNoticeRule
{
    public List<string> Check(PayslipNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PayslipNoticeCodeLengthRule(int MaxLength) : IPayslipNoticeRule
{
    public List<string> Check(PayslipNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipNoticeCodePrefixRule(string Prefix) : IPayslipNoticeRule
{
    public List<string> Check(PayslipNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipNoticeVersionRangeRule(int Min, int Max) : IPayslipNoticeRule
{
    public List<string> Check(PayslipNotice item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipNoticeRequiredCodeRule(string Message) : IPayslipNoticeRule
{
    public List<string> Check(PayslipNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip notice.</summary>
public sealed record PayslipNoticeValidator(List<IPayslipNoticeRule> Rules)
{
    public List<string> Validate(PayslipNotice item)
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

    public void RequireValid(PayslipNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipNotice> items)
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

/// <summary>Functions of payslip notice rules.</summary>
public static class PayslipNoticeRules
{
    public static PayslipNoticeValidator DefaultPayslipNoticeValidator()
    {
        return new PayslipNoticeValidator(
            new List<IPayslipNoticeRule>
            {
                new PayslipNoticeRequiredLabelRule("payslip notice needs an entry"),
                new PayslipNoticeAllowedDescriptionRule(new List<string>()),
                new PayslipNoticeCodeLengthRule(1000),
                new PayslipNoticeCodePrefixRule("OPS"),
                new PayslipNoticeVersionRangeRule(255, 1000),
                new PayslipNoticeRequiredCodeRule("payslip notice needs this field"),
            });
    }
}
