namespace Acme.Workforce.Talent.Nomination.Window;

/// <summary>A rule a nomination score must pass. Check returns its problems, or none.</summary>
public interface INominationScoreRule
{
    List<string> Check(NominationScore item);
}

public sealed record NominationScoreRequiredLabelRule(string Message) : INominationScoreRule
{
    public List<string> Check(NominationScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationScoreTeamLengthRule(int MaxLength) : INominationScoreRule
{
    public List<string> Check(NominationScore item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationScoreRequiredTeamRule(string Message) : INominationScoreRule
{
    public List<string> Check(NominationScore item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination score.</summary>
public sealed record NominationScoreValidator(List<INominationScoreRule> Rules)
{
    public List<string> Validate(NominationScore item)
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

    public void RequireValid(NominationScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationScore> items)
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

/// <summary>Functions of nomination score rules.</summary>
public static class NominationScoreRules
{
    public static NominationScoreValidator DefaultNominationScoreValidator()
    {
        return new NominationScoreValidator(
            new List<INominationScoreRule>
            {
                new NominationScoreRequiredLabelRule("nomination score needs this field"),
                new NominationScoreTeamLengthRule(80),
                new NominationScoreRequiredTeamRule("nomination score needs a value"),
            });
    }
}
