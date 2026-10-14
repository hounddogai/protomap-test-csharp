namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>A rule a leave request proposal must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestProposalRule
{
    List<string> Check(LeaveRequestProposal item);
}

public sealed record LeaveRequestProposalRequiredCodeRule(string Message) : ILeaveRequestProposalRule
{
    public List<string> Check(LeaveRequestProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestProposalAttemptsRangeRule(int Min, int Max) : ILeaveRequestProposalRule
{
    public List<string> Check(LeaveRequestProposal item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestProposalRequiredPhoneRule(string Message) : ILeaveRequestProposalRule
{
    public List<string> Check(LeaveRequestProposal item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request proposal.</summary>
public sealed record LeaveRequestProposalValidator(List<ILeaveRequestProposalRule> Rules)
{
    public List<string> Validate(LeaveRequestProposal item)
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

    public void RequireValid(LeaveRequestProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestProposal> items)
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

/// <summary>Functions of leave request proposal rules.</summary>
public static class LeaveRequestProposalRules
{
    public static LeaveRequestProposalValidator DefaultLeaveRequestProposalValidator()
    {
        return new LeaveRequestProposalValidator(
            new List<ILeaveRequestProposalRule>
            {
                new LeaveRequestProposalRequiredCodeRule("leave request proposal needs a value"),
                new LeaveRequestProposalAttemptsRangeRule(255, 1000),
                new LeaveRequestProposalRequiredPhoneRule("leave request proposal needs a value"),
            });
    }
}
