namespace Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>A rule a strict reorg draft must pass. Check returns its problems, or none.</summary>
public interface IStrictReorgDraftRule
{
    List<string> Check(StrictReorgDraft item);
}

public sealed record StrictReorgDraftCreatedAtRangeRule(int Min, int Max) : IStrictReorgDraftRule
{
    public List<string> Check(StrictReorgDraft item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictReorgDraftCodeLengthRule(int MaxLength) : IStrictReorgDraftRule
{
    public List<string> Check(StrictReorgDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgDraftRequiredCodeRule(string Message) : IStrictReorgDraftRule
{
    public List<string> Check(StrictReorgDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgDraftRequiredTeamRule(string Message) : IStrictReorgDraftRule
{
    public List<string> Check(StrictReorgDraft item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgDraftTeamLengthRule(int MaxLength) : IStrictReorgDraftRule
{
    public List<string> Check(StrictReorgDraft item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict reorg draft.</summary>
public sealed record StrictReorgDraftValidator(List<IStrictReorgDraftRule> Rules)
{
    public List<string> Validate(StrictReorgDraft item)
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

    public void RequireValid(StrictReorgDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReorgDraft> items)
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

/// <summary>Functions of strict reorg draft rules.</summary>
public static class StrictReorgDraftRules
{
    public static StrictReorgDraftValidator DefaultStrictReorgDraftValidator()
    {
        return new StrictReorgDraftValidator(
            new List<IStrictReorgDraftRule>
            {
                new StrictReorgDraftCreatedAtRangeRule(16, 86400),
                new StrictReorgDraftCodeLengthRule(16),
                new StrictReorgDraftRequiredCodeRule("strict reorg draft needs a value"),
                new StrictReorgDraftRequiredTeamRule("strict reorg draft needs a value"),
                new StrictReorgDraftTeamLengthRule(32),
            });
    }
}
