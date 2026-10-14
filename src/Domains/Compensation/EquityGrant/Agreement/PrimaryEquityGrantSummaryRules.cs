namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary equity grant summary must pass. Check returns its problems, or none.</summary>
public interface IPrimaryEquityGrantSummaryRule
{
    List<string> Check(PrimaryEquityGrantSummary item);
}

public sealed record PrimaryEquityGrantSummaryRequiredTitleRule(string Message) : IPrimaryEquityGrantSummaryRule
{
    public List<string> Check(PrimaryEquityGrantSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantSummaryAllowedCategoryRule(List<string> Allowed) : IPrimaryEquityGrantSummaryRule
{
    public List<string> Check(PrimaryEquityGrantSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is missing" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantSummaryBudgetRangeRule(double Min, double Max) : IPrimaryEquityGrantSummaryRule
{
    public List<string> Check(PrimaryEquityGrantSummary item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEquityGrantSummaryTenureMonthsRangeRule(int Min, int Max) : IPrimaryEquityGrantSummaryRule
{
    public List<string> Check(PrimaryEquityGrantSummary item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryEquityGrantSummaryCodeLengthRule(int MaxLength) : IPrimaryEquityGrantSummaryRule
{
    public List<string> Check(PrimaryEquityGrantSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary equity grant summary.</summary>
public sealed record PrimaryEquityGrantSummaryValidator(List<IPrimaryEquityGrantSummaryRule> Rules)
{
    public List<string> Validate(PrimaryEquityGrantSummary item)
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

    public void RequireValid(PrimaryEquityGrantSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryEquityGrantSummary> items)
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

/// <summary>Functions of primary equity grant summary rules.</summary>
public static class PrimaryEquityGrantSummaryRules
{
    public static PrimaryEquityGrantSummaryValidator DefaultPrimaryEquityGrantSummaryValidator()
    {
        return new PrimaryEquityGrantSummaryValidator(
            new List<IPrimaryEquityGrantSummaryRule>
            {
                new PrimaryEquityGrantSummaryRequiredTitleRule("primary equity grant summary needs this field"),
                new PrimaryEquityGrantSummaryAllowedCategoryRule(
                    new List<string> { "seasonal", "default", "strict" }),
                new PrimaryEquityGrantSummaryBudgetRangeRule(3.3, 2105.9),
                new PrimaryEquityGrantSummaryTenureMonthsRangeRule(64, 3600),
                new PrimaryEquityGrantSummaryCodeLengthRule(1000),
            });
    }
}
