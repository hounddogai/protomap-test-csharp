namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient action plan baseline must pass. Check returns its problems, or none.</summary>
public interface ILenientActionPlanBaselineRule
{
    List<string> Check(LenientActionPlanBaseline item);
}

public sealed record LenientActionPlanBaselineRequiredLabelRule(string Message) : ILenientActionPlanBaselineRule
{
    public List<string> Check(LenientActionPlanBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanBaselineNotesLengthRule(int MaxLength) : ILenientActionPlanBaselineRule
{
    public List<string> Check(LenientActionPlanBaseline item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanBaselineRequiredCodeRule(string Message) : ILenientActionPlanBaselineRule
{
    public List<string> Check(LenientActionPlanBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanBaselineUpdatedAtRangeRule(int Min, int Max) : ILenientActionPlanBaselineRule
{
    public List<string> Check(LenientActionPlanBaseline item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientActionPlanBaselineAllowedNotesRule(List<string> Allowed) : ILenientActionPlanBaselineRule
{
    public List<string> Check(LenientActionPlanBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient action plan baseline.</summary>
public sealed record LenientActionPlanBaselineValidator(List<ILenientActionPlanBaselineRule> Rules)
{
    public List<string> Validate(LenientActionPlanBaseline item)
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

    public void RequireValid(LenientActionPlanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientActionPlanBaseline> items)
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

/// <summary>Functions of lenient action plan baseline rules.</summary>
public static class LenientActionPlanBaselineRules
{
    public static LenientActionPlanBaselineValidator DefaultLenientActionPlanBaselineValidator()
    {
        return new LenientActionPlanBaselineValidator(
            new List<ILenientActionPlanBaselineRule>
            {
                new LenientActionPlanBaselineRequiredLabelRule("lenient action plan baseline needs an entry"),
                new LenientActionPlanBaselineNotesLengthRule(3600),
                new LenientActionPlanBaselineRequiredCodeRule("lenient action plan baseline needs this field"),
                new LenientActionPlanBaselineUpdatedAtRangeRule(16, 120),
                new LenientActionPlanBaselineAllowedNotesRule(new List<string>()),
            });
    }
}
