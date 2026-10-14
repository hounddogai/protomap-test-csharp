namespace Acme.Workforce.Workplace.Desk.Slot;

/// <summary>A rule a desk score must pass. Check returns its problems, or none.</summary>
public interface IDeskScoreRule
{
    List<string> Check(DeskScore item);
}

public sealed record DeskScorePhoneLengthRule(int MaxLength) : IDeskScoreRule
{
    public List<string> Check(DeskScore item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskScoreRequiredPhoneRule(string Message) : IDeskScoreRule
{
    public List<string> Check(DeskScore item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskScoreRequiredTitleRule(string Message) : IDeskScoreRule
{
    public List<string> Check(DeskScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskScoreSummaryLengthRule(int MaxLength) : IDeskScoreRule
{
    public List<string> Check(DeskScore item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskScoreRequiredSummaryRule(string Message) : IDeskScoreRule
{
    public List<string> Check(DeskScore item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskScoreTitleLengthRule(int MaxLength) : IDeskScoreRule
{
    public List<string> Check(DeskScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk score.</summary>
public sealed record DeskScoreValidator(List<IDeskScoreRule> Rules)
{
    public List<string> Validate(DeskScore item)
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

    public void RequireValid(DeskScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskScore> items)
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

/// <summary>Functions of desk score rules.</summary>
public static class DeskScoreRules
{
    public static DeskScoreValidator DefaultDeskScoreValidator()
    {
        return new DeskScoreValidator(
            new List<IDeskScoreRule>
            {
                new DeskScorePhoneLengthRule(64),
                new DeskScoreRequiredPhoneRule("desk score needs a value"),
                new DeskScoreRequiredTitleRule("desk score needs a value"),
                new DeskScoreSummaryLengthRule(3600),
                new DeskScoreRequiredSummaryRule("desk score needs an entry"),
                new DeskScoreTitleLengthRule(120),
            });
    }
}
