namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>A rule a capped visitor window must pass. Check returns its problems, or none.</summary>
public interface ICappedVisitorWindowRule
{
    List<string> Check(CappedVisitorWindow item);
}

public sealed record CappedVisitorWindowCodeLengthRule(int MaxLength) : ICappedVisitorWindowRule
{
    public List<string> Check(CappedVisitorWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorWindowRequiredSummaryRule(string Message) : ICappedVisitorWindowRule
{
    public List<string> Check(CappedVisitorWindow item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorWindowPriorityRangeRule(int Min, int Max) : ICappedVisitorWindowRule
{
    public List<string> Check(CappedVisitorWindow item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped visitor window.</summary>
public sealed record CappedVisitorWindowValidator(List<ICappedVisitorWindowRule> Rules)
{
    public List<string> Validate(CappedVisitorWindow item)
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

    public void RequireValid(CappedVisitorWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedVisitorWindow> items)
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

/// <summary>Functions of capped visitor window rules.</summary>
public static class CappedVisitorWindowRules
{
    public static CappedVisitorWindowValidator DefaultCappedVisitorWindowValidator()
    {
        return new CappedVisitorWindowValidator(
            new List<ICappedVisitorWindowRule>
            {
                new CappedVisitorWindowCodeLengthRule(1000),
                new CappedVisitorWindowRequiredSummaryRule("capped visitor window needs this field"),
                new CappedVisitorWindowPriorityRangeRule(80, 255),
            });
    }
}
