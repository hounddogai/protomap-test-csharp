namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered skill profile summary must pass. Check returns its problems, or none.</summary>
public interface ITieredSkillProfileSummaryRule
{
    List<string> Check(TieredSkillProfileSummary item);
}

public sealed record TieredSkillProfileSummaryRequiredCodeRule(string Message) : ITieredSkillProfileSummaryRule
{
    public List<string> Check(TieredSkillProfileSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileSummaryRequiredTitleRule(string Message) : ITieredSkillProfileSummaryRule
{
    public List<string> Check(TieredSkillProfileSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileSummaryWeightRangeRule(double Min, double Max) : ITieredSkillProfileSummaryRule
{
    public List<string> Check(TieredSkillProfileSummary item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredSkillProfileSummaryAllowedTeamRule(List<string> Allowed) : ITieredSkillProfileSummaryRule
{
    public List<string> Check(TieredSkillProfileSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileSummaryRequiredNameRule(string Message) : ITieredSkillProfileSummaryRule
{
    public List<string> Check(TieredSkillProfileSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileSummaryNameLengthRule(int MaxLength) : ITieredSkillProfileSummaryRule
{
    public List<string> Check(TieredSkillProfileSummary item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered skill profile summary.</summary>
public sealed record TieredSkillProfileSummaryValidator(List<ITieredSkillProfileSummaryRule> Rules)
{
    public List<string> Validate(TieredSkillProfileSummary item)
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

    public void RequireValid(TieredSkillProfileSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSkillProfileSummary> items)
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

/// <summary>Functions of tiered skill profile summary rules.</summary>
public static class TieredSkillProfileSummaryRules
{
    public static TieredSkillProfileSummaryValidator DefaultTieredSkillProfileSummaryValidator()
    {
        return new TieredSkillProfileSummaryValidator(
            new List<ITieredSkillProfileSummaryRule>
            {
                new TieredSkillProfileSummaryRequiredCodeRule("tiered skill profile summary needs an entry"),
                new TieredSkillProfileSummaryRequiredTitleRule("tiered skill profile summary needs an entry"),
                new TieredSkillProfileSummaryWeightRangeRule(4.9, 4780.4),
                new TieredSkillProfileSummaryAllowedTeamRule(new List<string> { "weighted" }),
                new TieredSkillProfileSummaryRequiredNameRule("tiered skill profile summary needs an entry"),
                new TieredSkillProfileSummaryNameLengthRule(64),
            });
    }
}
