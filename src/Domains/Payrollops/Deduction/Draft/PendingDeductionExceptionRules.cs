namespace Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>A rule a pending deduction exception must pass. Check returns its problems, or none.</summary>
public interface IPendingDeductionExceptionRule
{
    List<string> Check(PendingDeductionException item);
}

public sealed record PendingDeductionExceptionRequiredLabelRule(string Message) : IPendingDeductionExceptionRule
{
    public List<string> Check(PendingDeductionException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingDeductionExceptionLabelLengthRule(int MaxLength) : IPendingDeductionExceptionRule
{
    public List<string> Check(PendingDeductionException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeductionExceptionCodeLengthRule(int MaxLength) : IPendingDeductionExceptionRule
{
    public List<string> Check(PendingDeductionException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeductionExceptionRequiredCodeRule(string Message) : IPendingDeductionExceptionRule
{
    public List<string> Check(PendingDeductionException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingDeductionExceptionLeaveDaysRangeRule(int Min, int Max) : IPendingDeductionExceptionRule
{
    public List<string> Check(PendingDeductionException item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending deduction exception.</summary>
public sealed record PendingDeductionExceptionValidator(List<IPendingDeductionExceptionRule> Rules)
{
    public List<string> Validate(PendingDeductionException item)
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

    public void RequireValid(PendingDeductionException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingDeductionException> items)
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

/// <summary>Functions of pending deduction exception rules.</summary>
public static class PendingDeductionExceptionRules
{
    public static PendingDeductionExceptionValidator DefaultPendingDeductionExceptionValidator()
    {
        return new PendingDeductionExceptionValidator(
            new List<IPendingDeductionExceptionRule>
            {
                new PendingDeductionExceptionRequiredLabelRule("pending deduction exception needs a value"),
                new PendingDeductionExceptionLabelLengthRule(255),
                new PendingDeductionExceptionCodeLengthRule(16),
                new PendingDeductionExceptionRequiredCodeRule("pending deduction exception needs a value"),
                new PendingDeductionExceptionLeaveDaysRangeRule(16, 1000),
            });
    }
}
