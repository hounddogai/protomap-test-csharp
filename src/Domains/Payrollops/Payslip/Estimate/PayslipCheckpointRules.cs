namespace Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>A rule a payslip checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPayslipCheckpointRule
{
    List<string> Check(PayslipCheckpoint item);
}

public sealed record PayslipCheckpointCodeLengthRule(int MaxLength) : IPayslipCheckpointRule
{
    public List<string> Check(PayslipCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipCheckpointCodePrefixRule(string Prefix) : IPayslipCheckpointRule
{
    public List<string> Check(PayslipCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipCheckpointNotesLengthRule(int MaxLength) : IPayslipCheckpointRule
{
    public List<string> Check(PayslipCheckpoint item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipCheckpointWeightRangeRule(double Min, double Max) : IPayslipCheckpointRule
{
    public List<string> Check(PayslipCheckpoint item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipCheckpointRequiredNotesRule(string Message) : IPayslipCheckpointRule
{
    public List<string> Check(PayslipCheckpoint item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip checkpoint.</summary>
public sealed record PayslipCheckpointValidator(List<IPayslipCheckpointRule> Rules)
{
    public List<string> Validate(PayslipCheckpoint item)
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

    public void RequireValid(PayslipCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipCheckpoint> items)
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

/// <summary>Functions of payslip checkpoint rules.</summary>
public static class PayslipCheckpointRules
{
    public static PayslipCheckpointValidator DefaultPayslipCheckpointValidator()
    {
        return new PayslipCheckpointValidator(
            new List<IPayslipCheckpointRule>
            {
                new PayslipCheckpointCodeLengthRule(32),
                new PayslipCheckpointCodePrefixRule("Q"),
                new PayslipCheckpointNotesLengthRule(120),
                new PayslipCheckpointWeightRangeRule(0.8, 308.0),
                new PayslipCheckpointRequiredNotesRule("payslip checkpoint needs an entry"),
            });
    }
}
