namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip summary must pass. Check returns its problems, or none.</summary>
public interface IPayslipSummaryRule
{
    List<string> Check(PayslipSummary item);
}

public sealed record PayslipSummaryRequiredTitleRule(string Message) : IPayslipSummaryRule
{
    public List<string> Check(PayslipSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipSummaryRateRangeRule(double Min, double Max) : IPayslipSummaryRule
{
    public List<string> Check(PayslipSummary item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipSummaryArchivedGuardRule(int Limit) : IPayslipSummaryRule
{
    public List<string> Check(PayslipSummary item)
    {
        if (item.Archived && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PayslipSummaryRequiredChannelRule(string Message) : IPayslipSummaryRule
{
    public List<string> Check(PayslipSummary item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipSummaryAllowedChannelRule(List<string> Allowed) : IPayslipSummaryRule
{
    public List<string> Check(PayslipSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip summary.</summary>
public sealed record PayslipSummaryValidator(List<IPayslipSummaryRule> Rules)
{
    public List<string> Validate(PayslipSummary item)
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

    public void RequireValid(PayslipSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipSummary> items)
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

/// <summary>Functions of payslip summary rules.</summary>
public static class PayslipSummaryRules
{
    public static PayslipSummaryValidator DefaultPayslipSummaryValidator()
    {
        return new PayslipSummaryValidator(
            new List<IPayslipSummaryRule>
            {
                new PayslipSummaryRequiredTitleRule("payslip summary needs an entry"),
                new PayslipSummaryRateRangeRule(7.0, 414.5),
                new PayslipSummaryArchivedGuardRule(1000),
                new PayslipSummaryRequiredChannelRule("payslip summary needs this field"),
                new PayslipSummaryAllowedChannelRule(new List<string> { "tiered", "fallback" }),
            });
    }
}
