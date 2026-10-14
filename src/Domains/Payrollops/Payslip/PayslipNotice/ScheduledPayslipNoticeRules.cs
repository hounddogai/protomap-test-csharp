namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>A rule a scheduled payslip notice must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayslipNoticeRule
{
    List<string> Check(ScheduledPayslipNotice item);
}

public sealed record ScheduledPayslipNoticeNameLengthRule(int MaxLength) : IScheduledPayslipNoticeRule
{
    public List<string> Check(ScheduledPayslipNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipNoticeRevisionRangeRule(int Min, int Max) : IScheduledPayslipNoticeRule
{
    public List<string> Check(ScheduledPayslipNotice item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayslipNoticeRequiredNameRule(string Message) : IScheduledPayslipNoticeRule
{
    public List<string> Check(ScheduledPayslipNotice item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled payslip notice.</summary>
public sealed record ScheduledPayslipNoticeValidator(List<IScheduledPayslipNoticeRule> Rules)
{
    public List<string> Validate(ScheduledPayslipNotice item)
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

    public void RequireValid(ScheduledPayslipNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayslipNotice> items)
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

/// <summary>Functions of scheduled payslip notice rules.</summary>
public static class ScheduledPayslipNoticeRules
{
    public static ScheduledPayslipNoticeValidator DefaultScheduledPayslipNoticeValidator()
    {
        return new ScheduledPayslipNoticeValidator(
            new List<IScheduledPayslipNoticeRule>
            {
                new ScheduledPayslipNoticeNameLengthRule(1000),
                new ScheduledPayslipNoticeRevisionRangeRule(80, 120),
                new ScheduledPayslipNoticeRequiredNameRule("scheduled payslip notice needs an entry"),
            });
    }
}
