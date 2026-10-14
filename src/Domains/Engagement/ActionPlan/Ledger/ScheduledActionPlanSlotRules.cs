namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>A rule a scheduled action plan slot must pass. Check returns its problems, or none.</summary>
public interface IScheduledActionPlanSlotRule
{
    List<string> Check(ScheduledActionPlanSlot item);
}

public sealed record ScheduledActionPlanSlotTenureMonthsRangeRule(int Min, int Max) : IScheduledActionPlanSlotRule
{
    public List<string> Check(ScheduledActionPlanSlot item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledActionPlanSlotManagerIdLengthRule(int MaxLength) : IScheduledActionPlanSlotRule
{
    public List<string> Check(ScheduledActionPlanSlot item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledActionPlanSlotApprovedGuardRule(int Limit) : IScheduledActionPlanSlotRule
{
    public List<string> Check(ScheduledActionPlanSlot item)
    {
        if (item.Approved && item.TenureMonths > Limit)
        {
            return new List<string> { $"tenure months {item.TenureMonths} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledActionPlanSlotRequiredDepartmentRule(string Message) : IScheduledActionPlanSlotRule
{
    public List<string> Check(ScheduledActionPlanSlot item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledActionPlanSlotWeightRangeRule(double Min, double Max) : IScheduledActionPlanSlotRule
{
    public List<string> Check(ScheduledActionPlanSlot item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled action plan slot.</summary>
public sealed record ScheduledActionPlanSlotValidator(List<IScheduledActionPlanSlotRule> Rules)
{
    public List<string> Validate(ScheduledActionPlanSlot item)
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

    public void RequireValid(ScheduledActionPlanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledActionPlanSlot> items)
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

/// <summary>Functions of scheduled action plan slot rules.</summary>
public static class ScheduledActionPlanSlotRules
{
    public static ScheduledActionPlanSlotValidator DefaultScheduledActionPlanSlotValidator()
    {
        return new ScheduledActionPlanSlotValidator(
            new List<IScheduledActionPlanSlotRule>
            {
                new ScheduledActionPlanSlotTenureMonthsRangeRule(64, 255),
                new ScheduledActionPlanSlotManagerIdLengthRule(120),
                new ScheduledActionPlanSlotApprovedGuardRule(64),
                new ScheduledActionPlanSlotRequiredDepartmentRule("scheduled action plan slot needs an entry"),
                new ScheduledActionPlanSlotWeightRangeRule(8.2, 638.5),
            });
    }
}
