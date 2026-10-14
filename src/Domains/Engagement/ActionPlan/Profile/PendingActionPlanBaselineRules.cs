namespace Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>A rule a pending action plan baseline must pass. Check returns its problems, or none.</summary>
public interface IPendingActionPlanBaselineRule
{
    List<string> Check(PendingActionPlanBaseline item);
}

public sealed record PendingActionPlanBaselineNameLengthRule(int MaxLength) : IPendingActionPlanBaselineRule
{
    public List<string> Check(PendingActionPlanBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanBaselineRevisionRangeRule(int Min, int Max) : IPendingActionPlanBaselineRule
{
    public List<string> Check(PendingActionPlanBaseline item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanBaselineEmployeeIdLengthRule(int MaxLength) : IPendingActionPlanBaselineRule
{
    public List<string> Check(PendingActionPlanBaseline item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanBaselineRequiredLabelRule(string Message) : IPendingActionPlanBaselineRule
{
    public List<string> Check(PendingActionPlanBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanBaselineAttemptsRangeRule(int Min, int Max) : IPendingActionPlanBaselineRule
{
    public List<string> Check(PendingActionPlanBaseline item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanBaselineWindowDaysRangeRule(int Min, int Max) : IPendingActionPlanBaselineRule
{
    public List<string> Check(PendingActionPlanBaseline item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending action plan baseline.</summary>
public sealed record PendingActionPlanBaselineValidator(List<IPendingActionPlanBaselineRule> Rules)
{
    public List<string> Validate(PendingActionPlanBaseline item)
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

    public void RequireValid(PendingActionPlanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingActionPlanBaseline> items)
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

/// <summary>Functions of pending action plan baseline rules.</summary>
public static class PendingActionPlanBaselineRules
{
    public static PendingActionPlanBaselineValidator DefaultPendingActionPlanBaselineValidator()
    {
        return new PendingActionPlanBaselineValidator(
            new List<IPendingActionPlanBaselineRule>
            {
                new PendingActionPlanBaselineNameLengthRule(255),
                new PendingActionPlanBaselineRevisionRangeRule(64, 120),
                new PendingActionPlanBaselineEmployeeIdLengthRule(1000),
                new PendingActionPlanBaselineRequiredLabelRule("pending action plan baseline needs a value"),
                new PendingActionPlanBaselineAttemptsRangeRule(120, 86400),
                new PendingActionPlanBaselineWindowDaysRangeRule(120, 120),
            });
    }
}
