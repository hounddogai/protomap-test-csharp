namespace Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>A rule a leave request ledger must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestLedgerRule
{
    List<string> Check(LeaveRequestLedger item);
}

public sealed record LeaveRequestLedgerRequiredLabelRule(string Message) : ILeaveRequestLedgerRule
{
    public List<string> Check(LeaveRequestLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestLedgerAttemptsRangeRule(int Min, int Max) : ILeaveRequestLedgerRule
{
    public List<string> Check(LeaveRequestLedger item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestLedgerLabelLengthRule(int MaxLength) : ILeaveRequestLedgerRule
{
    public List<string> Check(LeaveRequestLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestLedgerRequiredNameRule(string Message) : ILeaveRequestLedgerRule
{
    public List<string> Check(LeaveRequestLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request ledger.</summary>
public sealed record LeaveRequestLedgerValidator(List<ILeaveRequestLedgerRule> Rules)
{
    public List<string> Validate(LeaveRequestLedger item)
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

    public void RequireValid(LeaveRequestLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestLedger> items)
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

/// <summary>Functions of leave request ledger rules.</summary>
public static class LeaveRequestLedgerRules
{
    public static LeaveRequestLedgerValidator DefaultLeaveRequestLedgerValidator()
    {
        return new LeaveRequestLedgerValidator(
            new List<ILeaveRequestLedgerRule>
            {
                new LeaveRequestLedgerRequiredLabelRule("leave request ledger needs this field"),
                new LeaveRequestLedgerAttemptsRangeRule(32, 64),
                new LeaveRequestLedgerLabelLengthRule(86400),
                new LeaveRequestLedgerRequiredNameRule("leave request ledger needs a value"),
            });
    }
}
