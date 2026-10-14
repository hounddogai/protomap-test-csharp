namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>A rule a payslip batch must pass. Check returns its problems, or none.</summary>
public interface IPayslipBatchRule
{
    List<string> Check(PayslipBatch item);
}

public sealed record PayslipBatchCodeLengthRule(int MaxLength) : IPayslipBatchRule
{
    public List<string> Check(PayslipBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipBatchRequiredCodeRule(string Message) : IPayslipBatchRule
{
    public List<string> Check(PayslipBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipBatchNotesLengthRule(int MaxLength) : IPayslipBatchRule
{
    public List<string> Check(PayslipBatch item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipBatchScoreRangeRule(double Min, double Max) : IPayslipBatchRule
{
    public List<string> Check(PayslipBatch item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip batch.</summary>
public sealed record PayslipBatchValidator(List<IPayslipBatchRule> Rules)
{
    public List<string> Validate(PayslipBatch item)
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

    public void RequireValid(PayslipBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipBatch> items)
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

/// <summary>Functions of payslip batch rules.</summary>
public static class PayslipBatchRules
{
    public static PayslipBatchValidator DefaultPayslipBatchValidator()
    {
        return new PayslipBatchValidator(
            new List<IPayslipBatchRule>
            {
                new PayslipBatchCodeLengthRule(80),
                new PayslipBatchRequiredCodeRule("payslip batch needs a value"),
                new PayslipBatchNotesLengthRule(120),
                new PayslipBatchScoreRangeRule(6.9, 1065.1),
            });
    }
}
