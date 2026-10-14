namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestCheckpointRule
{
    List<string> Check(LeaveRequestCheckpoint item);
}

public sealed record LeaveRequestCheckpointAllowedCostCenterRule(List<string> Allowed) : ILeaveRequestCheckpointRule
{
    public List<string> Check(LeaveRequestCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestCheckpointRequiredCodeRule(string Message) : ILeaveRequestCheckpointRule
{
    public List<string> Check(LeaveRequestCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestCheckpointRequiredTitleRule(string Message) : ILeaveRequestCheckpointRule
{
    public List<string> Check(LeaveRequestCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestCheckpointCodeLengthRule(int MaxLength) : ILeaveRequestCheckpointRule
{
    public List<string> Check(LeaveRequestCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestCheckpointCodePrefixRule(string Prefix) : ILeaveRequestCheckpointRule
{
    public List<string> Check(LeaveRequestCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request checkpoint.</summary>
public sealed record LeaveRequestCheckpointValidator(List<ILeaveRequestCheckpointRule> Rules)
{
    public List<string> Validate(LeaveRequestCheckpoint item)
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

    public void RequireValid(LeaveRequestCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestCheckpoint> items)
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

/// <summary>Functions of leave request checkpoint rules.</summary>
public static class LeaveRequestCheckpointRules
{
    public static LeaveRequestCheckpointValidator DefaultLeaveRequestCheckpointValidator()
    {
        return new LeaveRequestCheckpointValidator(
            new List<ILeaveRequestCheckpointRule>
            {
                new LeaveRequestCheckpointAllowedCostCenterRule(new List<string>()),
                new LeaveRequestCheckpointRequiredCodeRule("leave request checkpoint needs a value"),
                new LeaveRequestCheckpointRequiredTitleRule("leave request checkpoint needs an entry"),
                new LeaveRequestCheckpointCodeLengthRule(80),
                new LeaveRequestCheckpointCodePrefixRule("OPS"),
            });
    }
}
