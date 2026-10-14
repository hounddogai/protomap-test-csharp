namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work threshold must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkThresholdRule
{
    List<string> Check(StatementOfWorkThreshold item);
}

public sealed record StatementOfWorkThresholdStartsAtRangeRule(int Min, int Max) : IStatementOfWorkThresholdRule
{
    public List<string> Check(StatementOfWorkThreshold item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkThresholdGraceDaysRangeRule(int Min, int Max) : IStatementOfWorkThresholdRule
{
    public List<string> Check(StatementOfWorkThreshold item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkThresholdAllowedLabelRule(List<string> Allowed) : IStatementOfWorkThresholdRule
{
    public List<string> Check(StatementOfWorkThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkThresholdDiscountRangeRule(double Min, double Max) : IStatementOfWorkThresholdRule
{
    public List<string> Check(StatementOfWorkThreshold item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkThresholdRequiredLabelRule(string Message) : IStatementOfWorkThresholdRule
{
    public List<string> Check(StatementOfWorkThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work threshold.</summary>
public sealed record StatementOfWorkThresholdValidator(List<IStatementOfWorkThresholdRule> Rules)
{
    public List<string> Validate(StatementOfWorkThreshold item)
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

    public void RequireValid(StatementOfWorkThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkThreshold> items)
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

/// <summary>Functions of statement of work threshold rules.</summary>
public static class StatementOfWorkThresholdRules
{
    public static StatementOfWorkThresholdValidator DefaultStatementOfWorkThresholdValidator()
    {
        return new StatementOfWorkThresholdValidator(
            new List<IStatementOfWorkThresholdRule>
            {
                new StatementOfWorkThresholdStartsAtRangeRule(64, 1000),
                new StatementOfWorkThresholdGraceDaysRangeRule(1000, 3600),
                new StatementOfWorkThresholdAllowedLabelRule(new List<string> { "batched", "adaptive" }),
                new StatementOfWorkThresholdDiscountRangeRule(7.3, 526.3),
                new StatementOfWorkThresholdRequiredLabelRule("statement of work threshold needs this field"),
            });
    }
}
