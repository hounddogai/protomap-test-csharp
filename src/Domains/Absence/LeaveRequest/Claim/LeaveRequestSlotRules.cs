namespace Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>A rule a leave request slot must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestSlotRule
{
    List<string> Check(LeaveRequestSlot item);
}

public sealed record LeaveRequestSlotQuantityRangeRule(int Min, int Max) : ILeaveRequestSlotRule
{
    public List<string> Check(LeaveRequestSlot item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestSlotRequiredCostCenterRule(string Message) : ILeaveRequestSlotRule
{
    public List<string> Check(LeaveRequestSlot item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSlotLabelLengthRule(int MaxLength) : ILeaveRequestSlotRule
{
    public List<string> Check(LeaveRequestSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSlotCostCenterLengthRule(int MaxLength) : ILeaveRequestSlotRule
{
    public List<string> Check(LeaveRequestSlot item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request slot.</summary>
public sealed record LeaveRequestSlotValidator(List<ILeaveRequestSlotRule> Rules)
{
    public List<string> Validate(LeaveRequestSlot item)
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

    public void RequireValid(LeaveRequestSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestSlot> items)
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

/// <summary>Functions of leave request slot rules.</summary>
public static class LeaveRequestSlotRules
{
    public static LeaveRequestSlotValidator DefaultLeaveRequestSlotValidator()
    {
        return new LeaveRequestSlotValidator(
            new List<ILeaveRequestSlotRule>
            {
                new LeaveRequestSlotQuantityRangeRule(32, 3600),
                new LeaveRequestSlotRequiredCostCenterRule("leave request slot needs an entry"),
                new LeaveRequestSlotLabelLengthRule(16),
                new LeaveRequestSlotCostCenterLengthRule(64),
            });
    }
}
