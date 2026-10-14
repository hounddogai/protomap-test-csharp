namespace Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>A rule a manual floor plan item must pass. Check returns its problems, or none.</summary>
public interface IManualFloorPlanItemRule
{
    List<string> Check(ManualFloorPlanItem item);
}

public sealed record ManualFloorPlanItemArchivedGuardRule(int Limit) : IManualFloorPlanItemRule
{
    public List<string> Check(ManualFloorPlanItem item)
    {
        if (item.Archived && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanItemOwnerIdLengthRule(int MaxLength) : IManualFloorPlanItemRule
{
    public List<string> Check(ManualFloorPlanItem item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanItemRequiredTitleRule(string Message) : IManualFloorPlanItemRule
{
    public List<string> Check(ManualFloorPlanItem item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanItemCodeLengthRule(int MaxLength) : IManualFloorPlanItemRule
{
    public List<string> Check(ManualFloorPlanItem item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanItemLeaveDaysRangeRule(int Min, int Max) : IManualFloorPlanItemRule
{
    public List<string> Check(ManualFloorPlanItem item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual floor plan item.</summary>
public sealed record ManualFloorPlanItemValidator(List<IManualFloorPlanItemRule> Rules)
{
    public List<string> Validate(ManualFloorPlanItem item)
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

    public void RequireValid(ManualFloorPlanItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualFloorPlanItem> items)
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

/// <summary>Functions of manual floor plan item rules.</summary>
public static class ManualFloorPlanItemRules
{
    public static ManualFloorPlanItemValidator DefaultManualFloorPlanItemValidator()
    {
        return new ManualFloorPlanItemValidator(
            new List<IManualFloorPlanItemRule>
            {
                new ManualFloorPlanItemArchivedGuardRule(255),
                new ManualFloorPlanItemOwnerIdLengthRule(86400),
                new ManualFloorPlanItemRequiredTitleRule("manual floor plan item needs a value"),
                new ManualFloorPlanItemCodeLengthRule(3600),
                new ManualFloorPlanItemLeaveDaysRangeRule(32, 3600),
            });
    }
}
