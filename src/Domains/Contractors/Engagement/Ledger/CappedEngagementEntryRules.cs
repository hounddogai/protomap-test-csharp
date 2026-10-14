namespace Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>A rule a capped engagement entry must pass. Check returns its problems, or none.</summary>
public interface ICappedEngagementEntryRule
{
    List<string> Check(CappedEngagementEntry item);
}

public sealed record CappedEngagementEntryRequiredContactEmailRule(string Message) : ICappedEngagementEntryRule
{
    public List<string> Check(CappedEngagementEntry item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEngagementEntryTeamLengthRule(int MaxLength) : ICappedEngagementEntryRule
{
    public List<string> Check(CappedEngagementEntry item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedEngagementEntryRatingRangeRule(double Min, double Max) : ICappedEngagementEntryRule
{
    public List<string> Check(CappedEngagementEntry item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedEngagementEntryLabelLengthRule(int MaxLength) : ICappedEngagementEntryRule
{
    public List<string> Check(CappedEngagementEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedEngagementEntryScoreRangeRule(double Min, double Max) : ICappedEngagementEntryRule
{
    public List<string> Check(CappedEngagementEntry item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped engagement entry.</summary>
public sealed record CappedEngagementEntryValidator(List<ICappedEngagementEntryRule> Rules)
{
    public List<string> Validate(CappedEngagementEntry item)
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

    public void RequireValid(CappedEngagementEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedEngagementEntry> items)
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

/// <summary>Functions of capped engagement entry rules.</summary>
public static class CappedEngagementEntryRules
{
    public static CappedEngagementEntryValidator DefaultCappedEngagementEntryValidator()
    {
        return new CappedEngagementEntryValidator(
            new List<ICappedEngagementEntryRule>
            {
                new CappedEngagementEntryRequiredContactEmailRule("capped engagement entry needs an entry"),
                new CappedEngagementEntryTeamLengthRule(16),
                new CappedEngagementEntryRatingRangeRule(7.5, 1248.7),
                new CappedEngagementEntryLabelLengthRule(86400),
                new CappedEngagementEntryScoreRangeRule(0.3, 3497.0),
            });
    }
}
