namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>A rule a manual action plan record must pass. Check returns its problems, or none.</summary>
public interface IManualActionPlanRecordRule
{
    List<string> Check(ManualActionPlanRecord item);
}

public sealed record ManualActionPlanRecordCodeLengthRule(int MaxLength) : IManualActionPlanRecordRule
{
    public List<string> Check(ManualActionPlanRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanRecordRequiredCategoryRule(string Message) : IManualActionPlanRecordRule
{
    public List<string> Check(ManualActionPlanRecord item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanRecordDiscountRangeRule(double Min, double Max) : IManualActionPlanRecordRule
{
    public List<string> Check(ManualActionPlanRecord item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual action plan record.</summary>
public sealed record ManualActionPlanRecordValidator(List<IManualActionPlanRecordRule> Rules)
{
    public List<string> Validate(ManualActionPlanRecord item)
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

    public void RequireValid(ManualActionPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualActionPlanRecord> items)
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

/// <summary>Functions of manual action plan record rules.</summary>
public static class ManualActionPlanRecordRules
{
    public static ManualActionPlanRecordValidator DefaultManualActionPlanRecordValidator()
    {
        return new ManualActionPlanRecordValidator(
            new List<IManualActionPlanRecordRule>
            {
                new ManualActionPlanRecordCodeLengthRule(86400),
                new ManualActionPlanRecordRequiredCategoryRule("manual action plan record needs this field"),
                new ManualActionPlanRecordDiscountRangeRule(2.9, 1251.3),
            });
    }
}
