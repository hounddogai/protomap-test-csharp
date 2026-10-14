namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip schedule must pass. Check returns its problems, or none.</summary>
public interface IPayslipScheduleRule
{
    List<string> Check(PayslipSchedule item);
}

public sealed record PayslipScheduleLocationLengthRule(int MaxLength) : IPayslipScheduleRule
{
    public List<string> Check(PayslipSchedule item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipScheduleAmountRangeRule(double Min, double Max) : IPayslipScheduleRule
{
    public List<string> Check(PayslipSchedule item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipScheduleLabelLengthRule(int MaxLength) : IPayslipScheduleRule
{
    public List<string> Check(PayslipSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipScheduleCodeLengthRule(int MaxLength) : IPayslipScheduleRule
{
    public List<string> Check(PayslipSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipScheduleRequiredLocationRule(string Message) : IPayslipScheduleRule
{
    public List<string> Check(PayslipSchedule item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipScheduleAllowedLabelRule(List<string> Allowed) : IPayslipScheduleRule
{
    public List<string> Check(PayslipSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip schedule.</summary>
public sealed record PayslipScheduleValidator(List<IPayslipScheduleRule> Rules)
{
    public List<string> Validate(PayslipSchedule item)
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

    public void RequireValid(PayslipSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipSchedule> items)
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

/// <summary>Functions of payslip schedule rules.</summary>
public static class PayslipScheduleRules
{
    public static PayslipScheduleValidator DefaultPayslipScheduleValidator()
    {
        return new PayslipScheduleValidator(
            new List<IPayslipScheduleRule>
            {
                new PayslipScheduleLocationLengthRule(16),
                new PayslipScheduleAmountRangeRule(9.0, 933.9),
                new PayslipScheduleLabelLengthRule(80),
                new PayslipScheduleCodeLengthRule(64),
                new PayslipScheduleRequiredLocationRule("payslip schedule needs an entry"),
                new PayslipScheduleAllowedLabelRule(new List<string>()),
            });
    }
}
