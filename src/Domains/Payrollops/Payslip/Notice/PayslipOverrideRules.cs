namespace Acme.Workforce.Payrollops.Payslip.Notice;

/// <summary>A rule a payslip override must pass. Check returns its problems, or none.</summary>
public interface IPayslipOverrideRule
{
    List<string> Check(PayslipOverride item);
}

public sealed record PayslipOverrideLeaveDaysRangeRule(int Min, int Max) : IPayslipOverrideRule
{
    public List<string> Check(PayslipOverride item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipOverrideRequiredLocationRule(string Message) : IPayslipOverrideRule
{
    public List<string> Check(PayslipOverride item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipOverrideCodePrefixRule(string Prefix) : IPayslipOverrideRule
{
    public List<string> Check(PayslipOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipOverrideCodeLengthRule(int MaxLength) : IPayslipOverrideRule
{
    public List<string> Check(PayslipOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipOverrideDiscountRangeRule(double Min, double Max) : IPayslipOverrideRule
{
    public List<string> Check(PayslipOverride item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a payslip override.</summary>
public sealed record PayslipOverrideValidator(List<IPayslipOverrideRule> Rules)
{
    public List<string> Validate(PayslipOverride item)
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

    public void RequireValid(PayslipOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipOverride> items)
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

/// <summary>Functions of payslip override rules.</summary>
public static class PayslipOverrideRules
{
    public static PayslipOverrideValidator DefaultPayslipOverrideValidator()
    {
        return new PayslipOverrideValidator(
            new List<IPayslipOverrideRule>
            {
                new PayslipOverrideLeaveDaysRangeRule(120, 3600),
                new PayslipOverrideRequiredLocationRule("payslip override needs an entry"),
                new PayslipOverrideCodePrefixRule("REF"),
                new PayslipOverrideCodeLengthRule(255),
                new PayslipOverrideDiscountRangeRule(7.5, 3631.9),
            });
    }
}
