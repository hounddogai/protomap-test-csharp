namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>A rule a strict timesheet estimate must pass. Check returns its problems, or none.</summary>
public interface IStrictTimesheetEstimateRule
{
    List<string> Check(StrictTimesheetEstimate item);
}

public sealed record StrictTimesheetEstimateCostCenterLengthRule(int MaxLength) : IStrictTimesheetEstimateRule
{
    public List<string> Check(StrictTimesheetEstimate item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTimesheetEstimateRequiredNotesRule(string Message) : IStrictTimesheetEstimateRule
{
    public List<string> Check(StrictTimesheetEstimate item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictTimesheetEstimateLabelLengthRule(int MaxLength) : IStrictTimesheetEstimateRule
{
    public List<string> Check(StrictTimesheetEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTimesheetEstimateRequiredCostCenterRule(string Message) : IStrictTimesheetEstimateRule
{
    public List<string> Check(StrictTimesheetEstimate item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictTimesheetEstimateCodeLengthRule(int MaxLength) : IStrictTimesheetEstimateRule
{
    public List<string> Check(StrictTimesheetEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict timesheet estimate.</summary>
public sealed record StrictTimesheetEstimateValidator(List<IStrictTimesheetEstimateRule> Rules)
{
    public List<string> Validate(StrictTimesheetEstimate item)
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

    public void RequireValid(StrictTimesheetEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictTimesheetEstimate> items)
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

/// <summary>Functions of strict timesheet estimate rules.</summary>
public static class StrictTimesheetEstimateRules
{
    public static StrictTimesheetEstimateValidator DefaultStrictTimesheetEstimateValidator()
    {
        return new StrictTimesheetEstimateValidator(
            new List<IStrictTimesheetEstimateRule>
            {
                new StrictTimesheetEstimateCostCenterLengthRule(32),
                new StrictTimesheetEstimateRequiredNotesRule("strict timesheet estimate needs an entry"),
                new StrictTimesheetEstimateLabelLengthRule(64),
                new StrictTimesheetEstimateRequiredCostCenterRule("strict timesheet estimate needs a value"),
                new StrictTimesheetEstimateCodeLengthRule(255),
            });
    }
}
