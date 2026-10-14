namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a deduction notice must pass. Check returns its problems, or none.</summary>
public interface IDeductionNoticeRule
{
    List<string> Check(DeductionNotice item);
}

public sealed record DeductionNoticeCostCenterLengthRule(int MaxLength) : IDeductionNoticeRule
{
    public List<string> Check(DeductionNotice item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionNoticeCodePrefixRule(string Prefix) : IDeductionNoticeRule
{
    public List<string> Check(DeductionNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionNoticeAllowedCodeRule(List<string> Allowed) : IDeductionNoticeRule
{
    public List<string> Check(DeductionNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record DeductionNoticeRequiredTitleRule(string Message) : IDeductionNoticeRule
{
    public List<string> Check(DeductionNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction notice.</summary>
public sealed record DeductionNoticeValidator(List<IDeductionNoticeRule> Rules)
{
    public List<string> Validate(DeductionNotice item)
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

    public void RequireValid(DeductionNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionNotice> items)
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

/// <summary>Functions of deduction notice rules.</summary>
public static class DeductionNoticeRules
{
    public static DeductionNoticeValidator DefaultDeductionNoticeValidator()
    {
        return new DeductionNoticeValidator(
            new List<IDeductionNoticeRule>
            {
                new DeductionNoticeCostCenterLengthRule(255),
                new DeductionNoticeCodePrefixRule("OPS"),
                new DeductionNoticeAllowedCodeRule(new List<string>()),
                new DeductionNoticeRequiredTitleRule("deduction notice needs a value"),
            });
    }
}
