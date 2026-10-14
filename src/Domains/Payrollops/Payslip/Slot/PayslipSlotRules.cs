namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip slot must pass. Check returns its problems, or none.</summary>
public interface IPayslipSlotRule
{
    List<string> Check(PayslipSlot item);
}

public sealed record PayslipSlotCodeLengthRule(int MaxLength) : IPayslipSlotRule
{
    public List<string> Check(PayslipSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipSlotRequiredDepartmentRule(string Message) : IPayslipSlotRule
{
    public List<string> Check(PayslipSlot item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipSlotAllowedDepartmentRule(List<string> Allowed) : IPayslipSlotRule
{
    public List<string> Check(PayslipSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PayslipSlotCodePrefixRule(string Prefix) : IPayslipSlotRule
{
    public List<string> Check(PayslipSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipSlotCeilingRangeRule(double Min, double Max) : IPayslipSlotRule
{
    public List<string> Check(PayslipSlot item)
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

/// <summary>Runs every rule of a payslip slot.</summary>
public sealed record PayslipSlotValidator(List<IPayslipSlotRule> Rules)
{
    public List<string> Validate(PayslipSlot item)
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

    public void RequireValid(PayslipSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipSlot> items)
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

/// <summary>Functions of payslip slot rules.</summary>
public static class PayslipSlotRules
{
    public static PayslipSlotValidator DefaultPayslipSlotValidator()
    {
        return new PayslipSlotValidator(
            new List<IPayslipSlotRule>
            {
                new PayslipSlotCodeLengthRule(255),
                new PayslipSlotRequiredDepartmentRule("payslip slot needs this field"),
                new PayslipSlotAllowedDepartmentRule(new List<string> { "batched", "global" }),
                new PayslipSlotCodePrefixRule("REF"),
                new PayslipSlotCeilingRangeRule(4.5, 2899.3),
            });
    }
}
