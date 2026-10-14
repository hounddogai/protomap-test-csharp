namespace Acme.Workforce.Payrollops.Garnishment.Review;

/// <summary>A rule a garnishment summary must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentSummaryRule
{
    List<string> Check(GarnishmentSummary item);
}

public sealed record GarnishmentSummaryLabelLengthRule(int MaxLength) : IGarnishmentSummaryRule
{
    public List<string> Check(GarnishmentSummary item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentSummaryCodeLengthRule(int MaxLength) : IGarnishmentSummaryRule
{
    public List<string> Check(GarnishmentSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentSummaryRequiredCodeRule(string Message) : IGarnishmentSummaryRule
{
    public List<string> Check(GarnishmentSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment summary.</summary>
public sealed record GarnishmentSummaryValidator(List<IGarnishmentSummaryRule> Rules)
{
    public List<string> Validate(GarnishmentSummary item)
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

    public void RequireValid(GarnishmentSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentSummary> items)
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

/// <summary>Functions of garnishment summary rules.</summary>
public static class GarnishmentSummaryRules
{
    public static GarnishmentSummaryValidator DefaultGarnishmentSummaryValidator()
    {
        return new GarnishmentSummaryValidator(
            new List<IGarnishmentSummaryRule>
            {
                new GarnishmentSummaryLabelLengthRule(32),
                new GarnishmentSummaryCodeLengthRule(255),
                new GarnishmentSummaryRequiredCodeRule("garnishment summary needs a value"),
            });
    }
}
