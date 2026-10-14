namespace Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>A rule a visitor summary must pass. Check returns its problems, or none.</summary>
public interface IVisitorSummaryRule
{
    List<string> Check(VisitorSummary item);
}

public sealed record VisitorSummarySourceLengthRule(int MaxLength) : IVisitorSummaryRule
{
    public List<string> Check(VisitorSummary item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorSummaryEndsAtRangeRule(int Min, int Max) : IVisitorSummaryRule
{
    public List<string> Check(VisitorSummary item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorSummaryTitleLengthRule(int MaxLength) : IVisitorSummaryRule
{
    public List<string> Check(VisitorSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorSummaryRequiredSourceRule(string Message) : IVisitorSummaryRule
{
    public List<string> Check(VisitorSummary item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor summary.</summary>
public sealed record VisitorSummaryValidator(List<IVisitorSummaryRule> Rules)
{
    public List<string> Validate(VisitorSummary item)
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

    public void RequireValid(VisitorSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorSummary> items)
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

/// <summary>Functions of visitor summary rules.</summary>
public static class VisitorSummaryRules
{
    public static VisitorSummaryValidator DefaultVisitorSummaryValidator()
    {
        return new VisitorSummaryValidator(
            new List<IVisitorSummaryRule>
            {
                new VisitorSummarySourceLengthRule(255),
                new VisitorSummaryEndsAtRangeRule(120, 255),
                new VisitorSummaryTitleLengthRule(16),
                new VisitorSummaryRequiredSourceRule("visitor summary needs this field"),
            });
    }
}
