namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global equity grant draft must pass. Check returns its problems, or none.</summary>
public interface IGlobalEquityGrantDraftRule
{
    List<string> Check(GlobalEquityGrantDraft item);
}

public sealed record GlobalEquityGrantDraftRequiredTeamRule(string Message) : IGlobalEquityGrantDraftRule
{
    public List<string> Check(GlobalEquityGrantDraft item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantDraftRequiredRegionRule(string Message) : IGlobalEquityGrantDraftRule
{
    public List<string> Check(GlobalEquityGrantDraft item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantDraftTitleLengthRule(int MaxLength) : IGlobalEquityGrantDraftRule
{
    public List<string> Check(GlobalEquityGrantDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantDraftRetriesRangeRule(int Min, int Max) : IGlobalEquityGrantDraftRule
{
    public List<string> Check(GlobalEquityGrantDraft item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantDraftCodeLengthRule(int MaxLength) : IGlobalEquityGrantDraftRule
{
    public List<string> Check(GlobalEquityGrantDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEquityGrantDraftAllowedRegionRule(List<string> Allowed) : IGlobalEquityGrantDraftRule
{
    public List<string> Check(GlobalEquityGrantDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global equity grant draft.</summary>
public sealed record GlobalEquityGrantDraftValidator(List<IGlobalEquityGrantDraftRule> Rules)
{
    public List<string> Validate(GlobalEquityGrantDraft item)
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

    public void RequireValid(GlobalEquityGrantDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalEquityGrantDraft> items)
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

/// <summary>Functions of global equity grant draft rules.</summary>
public static class GlobalEquityGrantDraftRules
{
    public static GlobalEquityGrantDraftValidator DefaultGlobalEquityGrantDraftValidator()
    {
        return new GlobalEquityGrantDraftValidator(
            new List<IGlobalEquityGrantDraftRule>
            {
                new GlobalEquityGrantDraftRequiredTeamRule("global equity grant draft needs this field"),
                new GlobalEquityGrantDraftRequiredRegionRule("global equity grant draft needs a value"),
                new GlobalEquityGrantDraftTitleLengthRule(120),
                new GlobalEquityGrantDraftRetriesRangeRule(32, 80),
                new GlobalEquityGrantDraftCodeLengthRule(255),
                new GlobalEquityGrantDraftAllowedRegionRule(new List<string> { "lenient", "seasonal" }),
            });
    }
}
