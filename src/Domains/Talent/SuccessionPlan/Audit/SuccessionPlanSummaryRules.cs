namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>A rule a succession plan summary must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanSummaryRule
{
    List<string> Check(SuccessionPlanSummary item);
}

public sealed record SuccessionPlanSummaryRequiredNameRule(string Message) : ISuccessionPlanSummaryRule
{
    public List<string> Check(SuccessionPlanSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanSummaryRequiredCodeRule(string Message) : ISuccessionPlanSummaryRule
{
    public List<string> Check(SuccessionPlanSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanSummaryRevisionRangeRule(int Min, int Max) : ISuccessionPlanSummaryRule
{
    public List<string> Check(SuccessionPlanSummary item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanSummaryRequiredLabelRule(string Message) : ISuccessionPlanSummaryRule
{
    public List<string> Check(SuccessionPlanSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan summary.</summary>
public sealed record SuccessionPlanSummaryValidator(List<ISuccessionPlanSummaryRule> Rules)
{
    public List<string> Validate(SuccessionPlanSummary item)
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

    public void RequireValid(SuccessionPlanSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanSummary> items)
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

/// <summary>Functions of succession plan summary rules.</summary>
public static class SuccessionPlanSummaryRules
{
    public static SuccessionPlanSummaryValidator DefaultSuccessionPlanSummaryValidator()
    {
        return new SuccessionPlanSummaryValidator(
            new List<ISuccessionPlanSummaryRule>
            {
                new SuccessionPlanSummaryRequiredNameRule("succession plan summary needs this field"),
                new SuccessionPlanSummaryRequiredCodeRule("succession plan summary needs an entry"),
                new SuccessionPlanSummaryRevisionRangeRule(120, 3600),
                new SuccessionPlanSummaryRequiredLabelRule("succession plan summary needs a value"),
            });
    }
}
