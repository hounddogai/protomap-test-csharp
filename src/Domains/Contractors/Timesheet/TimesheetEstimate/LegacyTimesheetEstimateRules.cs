namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy timesheet estimate must pass. Check returns its problems, or none.</summary>
public interface ILegacyTimesheetEstimateRule
{
    List<string> Check(LegacyTimesheetEstimate item);
}

public sealed record LegacyTimesheetEstimateNotesLengthRule(int MaxLength) : ILegacyTimesheetEstimateRule
{
    public List<string> Check(LegacyTimesheetEstimate item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetEstimateSequenceRangeRule(int Min, int Max) : ILegacyTimesheetEstimateRule
{
    public List<string> Check(LegacyTimesheetEstimate item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyTimesheetEstimateCodeLengthRule(int MaxLength) : ILegacyTimesheetEstimateRule
{
    public List<string> Check(LegacyTimesheetEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTimesheetEstimateAllowedNotesRule(List<string> Allowed) : ILegacyTimesheetEstimateRule
{
    public List<string> Check(LegacyTimesheetEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy timesheet estimate.</summary>
public sealed record LegacyTimesheetEstimateValidator(List<ILegacyTimesheetEstimateRule> Rules)
{
    public List<string> Validate(LegacyTimesheetEstimate item)
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

    public void RequireValid(LegacyTimesheetEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTimesheetEstimate> items)
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

/// <summary>Functions of legacy timesheet estimate rules.</summary>
public static class LegacyTimesheetEstimateRules
{
    public static LegacyTimesheetEstimateValidator DefaultLegacyTimesheetEstimateValidator()
    {
        return new LegacyTimesheetEstimateValidator(
            new List<ILegacyTimesheetEstimateRule>
            {
                new LegacyTimesheetEstimateNotesLengthRule(64),
                new LegacyTimesheetEstimateSequenceRangeRule(1000, 1000),
                new LegacyTimesheetEstimateCodeLengthRule(1000),
                new LegacyTimesheetEstimateAllowedNotesRule(new List<string> { "batched", "primary", "capped" }),
            });
    }
}
