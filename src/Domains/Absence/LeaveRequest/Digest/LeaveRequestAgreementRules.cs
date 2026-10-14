namespace Acme.Workforce.Absence.LeaveRequest.Digest;

/// <summary>A rule a leave request agreement must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestAgreementRule
{
    List<string> Check(LeaveRequestAgreement item);
}

public sealed record LeaveRequestAgreementCeilingRangeRule(double Min, double Max) : ILeaveRequestAgreementRule
{
    public List<string> Check(LeaveRequestAgreement item)
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

public sealed record LeaveRequestAgreementBatchSizeRangeRule(int Min, int Max) : ILeaveRequestAgreementRule
{
    public List<string> Check(LeaveRequestAgreement item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAgreementUpdatedAtRangeRule(int Min, int Max) : ILeaveRequestAgreementRule
{
    public List<string> Check(LeaveRequestAgreement item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a leave request agreement.</summary>
public sealed record LeaveRequestAgreementValidator(List<ILeaveRequestAgreementRule> Rules)
{
    public List<string> Validate(LeaveRequestAgreement item)
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

    public void RequireValid(LeaveRequestAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestAgreement> items)
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

/// <summary>Functions of leave request agreement rules.</summary>
public static class LeaveRequestAgreementRules
{
    public static LeaveRequestAgreementValidator DefaultLeaveRequestAgreementValidator()
    {
        return new LeaveRequestAgreementValidator(
            new List<ILeaveRequestAgreementRule>
            {
                new LeaveRequestAgreementCeilingRangeRule(6.8, 3273.4),
                new LeaveRequestAgreementBatchSizeRangeRule(255, 1000),
                new LeaveRequestAgreementUpdatedAtRangeRule(32, 1000),
            });
    }
}
