namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary engagement summary must pass. Check returns its problems, or none.</summary>
public interface IPrimaryEngagementSummaryRule
{
    List<string> Check(PrimaryEngagementSummary item);
}

public sealed record PrimaryEngagementSummaryGraceDaysRangeRule(int Min, int Max) : IPrimaryEngagementSummaryRule
{
    public List<string> Check(PrimaryEngagementSummary item)
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

public sealed record PrimaryEngagementSummaryWindowDaysRangeRule(int Min, int Max) : IPrimaryEngagementSummaryRule
{
    public List<string> Check(PrimaryEngagementSummary item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEngagementSummaryAllowedCodeRule(List<string> Allowed) : IPrimaryEngagementSummaryRule
{
    public List<string> Check(PrimaryEngagementSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary engagement summary.</summary>
public sealed record PrimaryEngagementSummaryValidator(List<IPrimaryEngagementSummaryRule> Rules)
{
    public List<string> Validate(PrimaryEngagementSummary item)
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

    public void RequireValid(PrimaryEngagementSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryEngagementSummary> items)
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

/// <summary>Functions of primary engagement summary rules.</summary>
public static class PrimaryEngagementSummaryRules
{
    public static PrimaryEngagementSummaryValidator DefaultPrimaryEngagementSummaryValidator()
    {
        return new PrimaryEngagementSummaryValidator(
            new List<IPrimaryEngagementSummaryRule>
            {
                new PrimaryEngagementSummaryGraceDaysRangeRule(120, 3600),
                new PrimaryEngagementSummaryWindowDaysRangeRule(64, 255),
                new PrimaryEngagementSummaryAllowedCodeRule(new List<string>()),
            });
    }
}
