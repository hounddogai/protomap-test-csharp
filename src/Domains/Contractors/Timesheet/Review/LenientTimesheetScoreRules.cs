namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient timesheet score must pass. Check returns its problems, or none.</summary>
public interface ILenientTimesheetScoreRule
{
    List<string> Check(LenientTimesheetScore item);
}

public sealed record LenientTimesheetScoreAllowedDescriptionRule(List<string> Allowed) : ILenientTimesheetScoreRule
{
    public List<string> Check(LenientTimesheetScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetScoreRequiredCodeRule(string Message) : ILenientTimesheetScoreRule
{
    public List<string> Check(LenientTimesheetScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetScoreCodePrefixRule(string Prefix) : ILenientTimesheetScoreRule
{
    public List<string> Check(LenientTimesheetScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient timesheet score.</summary>
public sealed record LenientTimesheetScoreValidator(List<ILenientTimesheetScoreRule> Rules)
{
    public List<string> Validate(LenientTimesheetScore item)
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

    public void RequireValid(LenientTimesheetScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientTimesheetScore> items)
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

/// <summary>Functions of lenient timesheet score rules.</summary>
public static class LenientTimesheetScoreRules
{
    public static LenientTimesheetScoreValidator DefaultLenientTimesheetScoreValidator()
    {
        return new LenientTimesheetScoreValidator(
            new List<ILenientTimesheetScoreRule>
            {
                new LenientTimesheetScoreAllowedDescriptionRule(new List<string>()),
                new LenientTimesheetScoreRequiredCodeRule("lenient timesheet score needs a value"),
                new LenientTimesheetScoreCodePrefixRule("REF"),
            });
    }
}
