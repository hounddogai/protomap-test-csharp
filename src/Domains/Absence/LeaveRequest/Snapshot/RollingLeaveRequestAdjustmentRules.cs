namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling leave request adjustment must pass. Check returns its problems, or none.</summary>
public interface IRollingLeaveRequestAdjustmentRule
{
    List<string> Check(RollingLeaveRequestAdjustment item);
}

public sealed record RollingLeaveRequestAdjustmentAllowedDepartmentRule(List<string> Allowed) : IRollingLeaveRequestAdjustmentRule
{
    public List<string> Check(RollingLeaveRequestAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RollingLeaveRequestAdjustmentCountryLengthRule(int MaxLength) : IRollingLeaveRequestAdjustmentRule
{
    public List<string> Check(RollingLeaveRequestAdjustment item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLeaveRequestAdjustmentRequiredCodeRule(string Message) : IRollingLeaveRequestAdjustmentRule
{
    public List<string> Check(RollingLeaveRequestAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLeaveRequestAdjustmentRequiredTitleRule(string Message) : IRollingLeaveRequestAdjustmentRule
{
    public List<string> Check(RollingLeaveRequestAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling leave request adjustment.</summary>
public sealed record RollingLeaveRequestAdjustmentValidator(List<IRollingLeaveRequestAdjustmentRule> Rules)
{
    public List<string> Validate(RollingLeaveRequestAdjustment item)
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

    public void RequireValid(RollingLeaveRequestAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingLeaveRequestAdjustment> items)
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

/// <summary>Functions of rolling leave request adjustment rules.</summary>
public static class RollingLeaveRequestAdjustmentRules
{
    public static RollingLeaveRequestAdjustmentValidator DefaultRollingLeaveRequestAdjustmentValidator()
    {
        return new RollingLeaveRequestAdjustmentValidator(
            new List<IRollingLeaveRequestAdjustmentRule>
            {
                new RollingLeaveRequestAdjustmentAllowedDepartmentRule(new List<string> { "primary" }),
                new RollingLeaveRequestAdjustmentCountryLengthRule(1000),
                new RollingLeaveRequestAdjustmentRequiredCodeRule(
                    "rolling leave request adjustment needs this field"),
                new RollingLeaveRequestAdjustmentRequiredTitleRule("rolling leave request adjustment needs an entry"),
            });
    }
}
