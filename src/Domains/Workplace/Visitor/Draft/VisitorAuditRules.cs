namespace Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>A rule a visitor audit must pass. Check returns its problems, or none.</summary>
public interface IVisitorAuditRule
{
    List<string> Check(VisitorAudit item);
}

public sealed record VisitorAuditThresholdRangeRule(double Min, double Max) : IVisitorAuditRule
{
    public List<string> Check(VisitorAudit item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorAuditPriorityRangeRule(int Min, int Max) : IVisitorAuditRule
{
    public List<string> Check(VisitorAudit item)
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

public sealed record VisitorAuditGraceDaysRangeRule(int Min, int Max) : IVisitorAuditRule
{
    public List<string> Check(VisitorAudit item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorAuditRequiredTimezoneRule(string Message) : IVisitorAuditRule
{
    public List<string> Check(VisitorAudit item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorAuditTimezoneLengthRule(int MaxLength) : IVisitorAuditRule
{
    public List<string> Check(VisitorAudit item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor audit.</summary>
public sealed record VisitorAuditValidator(List<IVisitorAuditRule> Rules)
{
    public List<string> Validate(VisitorAudit item)
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

    public void RequireValid(VisitorAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorAudit> items)
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

/// <summary>Functions of visitor audit rules.</summary>
public static class VisitorAuditRules
{
    public static VisitorAuditValidator DefaultVisitorAuditValidator()
    {
        return new VisitorAuditValidator(
            new List<IVisitorAuditRule>
            {
                new VisitorAuditThresholdRangeRule(6.5, 1752.3),
                new VisitorAuditPriorityRangeRule(255, 86400),
                new VisitorAuditGraceDaysRangeRule(255, 86400),
                new VisitorAuditRequiredTimezoneRule("visitor audit needs this field"),
                new VisitorAuditTimezoneLengthRule(3600),
            });
    }
}
