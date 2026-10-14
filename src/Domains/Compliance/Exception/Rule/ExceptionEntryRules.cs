namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception entry must pass. Check returns its problems, or none.</summary>
public interface IExceptionEntryRule
{
    List<string> Check(ExceptionEntry item);
}

public sealed record ExceptionEntryRequiredSourceRule(string Message) : IExceptionEntryRule
{
    public List<string> Check(ExceptionEntry item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionEntryDiscountRangeRule(double Min, double Max) : IExceptionEntryRule
{
    public List<string> Check(ExceptionEntry item)
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

public sealed record ExceptionEntryRequiredTitleRule(string Message) : IExceptionEntryRule
{
    public List<string> Check(ExceptionEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionEntryAllowedCostCenterRule(List<string> Allowed) : IExceptionEntryRule
{
    public List<string> Check(ExceptionEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception entry.</summary>
public sealed record ExceptionEntryValidator(List<IExceptionEntryRule> Rules)
{
    public List<string> Validate(ExceptionEntry item)
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

    public void RequireValid(ExceptionEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionEntry> items)
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

/// <summary>Functions of exception entry rules.</summary>
public static class ExceptionEntryRules
{
    public static ExceptionEntryValidator DefaultExceptionEntryValidator()
    {
        return new ExceptionEntryValidator(
            new List<IExceptionEntryRule>
            {
                new ExceptionEntryRequiredSourceRule("exception entry needs a value"),
                new ExceptionEntryDiscountRangeRule(2.6, 2407.6),
                new ExceptionEntryRequiredTitleRule("exception entry needs a value"),
                new ExceptionEntryAllowedCostCenterRule(new List<string> { "scheduled", "tiered", "scheduled" }),
            });
    }
}
