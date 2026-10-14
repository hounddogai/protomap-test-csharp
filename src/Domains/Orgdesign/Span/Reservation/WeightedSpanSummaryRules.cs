namespace Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>A rule a weighted span summary must pass. Check returns its problems, or none.</summary>
public interface IWeightedSpanSummaryRule
{
    List<string> Check(WeightedSpanSummary item);
}

public sealed record WeightedSpanSummaryTitleLengthRule(int MaxLength) : IWeightedSpanSummaryRule
{
    public List<string> Check(WeightedSpanSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSpanSummaryRequiredSourceRule(string Message) : IWeightedSpanSummaryRule
{
    public List<string> Check(WeightedSpanSummary item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedSpanSummaryRequiredTitleRule(string Message) : IWeightedSpanSummaryRule
{
    public List<string> Check(WeightedSpanSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted span summary.</summary>
public sealed record WeightedSpanSummaryValidator(List<IWeightedSpanSummaryRule> Rules)
{
    public List<string> Validate(WeightedSpanSummary item)
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

    public void RequireValid(WeightedSpanSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedSpanSummary> items)
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

/// <summary>Functions of weighted span summary rules.</summary>
public static class WeightedSpanSummaryRules
{
    public static WeightedSpanSummaryValidator DefaultWeightedSpanSummaryValidator()
    {
        return new WeightedSpanSummaryValidator(
            new List<IWeightedSpanSummaryRule>
            {
                new WeightedSpanSummaryTitleLengthRule(86400),
                new WeightedSpanSummaryRequiredSourceRule("weighted span summary needs this field"),
                new WeightedSpanSummaryRequiredTitleRule("weighted span summary needs this field"),
            });
    }
}
