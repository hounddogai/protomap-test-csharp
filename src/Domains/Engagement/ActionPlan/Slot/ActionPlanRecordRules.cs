namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan record must pass. Check returns its problems, or none.</summary>
public interface IActionPlanRecordRule
{
    List<string> Check(ActionPlanRecord item);
}

public sealed record ActionPlanRecordAttemptsRangeRule(int Min, int Max) : IActionPlanRecordRule
{
    public List<string> Check(ActionPlanRecord item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanRecordBatchSizeRangeRule(int Min, int Max) : IActionPlanRecordRule
{
    public List<string> Check(ActionPlanRecord item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanRecordAllowedNameRule(List<string> Allowed) : IActionPlanRecordRule
{
    public List<string> Check(ActionPlanRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanRecordRequiredNameRule(string Message) : IActionPlanRecordRule
{
    public List<string> Check(ActionPlanRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanRecordLabelLengthRule(int MaxLength) : IActionPlanRecordRule
{
    public List<string> Check(ActionPlanRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan record.</summary>
public sealed record ActionPlanRecordValidator(List<IActionPlanRecordRule> Rules)
{
    public List<string> Validate(ActionPlanRecord item)
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

    public void RequireValid(ActionPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanRecord> items)
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

/// <summary>Functions of action plan record rules.</summary>
public static class ActionPlanRecordRules
{
    public static ActionPlanRecordValidator DefaultActionPlanRecordValidator()
    {
        return new ActionPlanRecordValidator(
            new List<IActionPlanRecordRule>
            {
                new ActionPlanRecordAttemptsRangeRule(3600, 3600),
                new ActionPlanRecordBatchSizeRangeRule(1000, 86400),
                new ActionPlanRecordAllowedNameRule(new List<string> { "batched" }),
                new ActionPlanRecordRequiredNameRule("action plan record needs a value"),
                new ActionPlanRecordLabelLengthRule(16),
            });
    }
}
