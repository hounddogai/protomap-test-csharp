namespace Acme.Workforce.Engagement.TownHall.Bundle;

/// <summary>A rule a pending town hall summary must pass. Check returns its problems, or none.</summary>
public interface IPendingTownHallSummaryRule
{
    List<string> Check(PendingTownHallSummary item);
}

public sealed record PendingTownHallSummaryGraceDaysRangeRule(int Min, int Max) : IPendingTownHallSummaryRule
{
    public List<string> Check(PendingTownHallSummary item)
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

public sealed record PendingTownHallSummaryCodePrefixRule(string Prefix) : IPendingTownHallSummaryRule
{
    public List<string> Check(PendingTownHallSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTownHallSummaryTitleLengthRule(int MaxLength) : IPendingTownHallSummaryRule
{
    public List<string> Check(PendingTownHallSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending town hall summary.</summary>
public sealed record PendingTownHallSummaryValidator(List<IPendingTownHallSummaryRule> Rules)
{
    public List<string> Validate(PendingTownHallSummary item)
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

    public void RequireValid(PendingTownHallSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTownHallSummary> items)
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

/// <summary>Functions of pending town hall summary rules.</summary>
public static class PendingTownHallSummaryRules
{
    public static PendingTownHallSummaryValidator DefaultPendingTownHallSummaryValidator()
    {
        return new PendingTownHallSummaryValidator(
            new List<IPendingTownHallSummaryRule>
            {
                new PendingTownHallSummaryGraceDaysRangeRule(255, 3600),
                new PendingTownHallSummaryCodePrefixRule("ACM"),
                new PendingTownHallSummaryTitleLengthRule(255),
            });
    }
}
