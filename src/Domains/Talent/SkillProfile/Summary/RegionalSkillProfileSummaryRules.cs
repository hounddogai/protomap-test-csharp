namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional skill profile summary must pass. Check returns its problems, or none.</summary>
public interface IRegionalSkillProfileSummaryRule
{
    List<string> Check(RegionalSkillProfileSummary item);
}

public sealed record RegionalSkillProfileSummaryPhoneLengthRule(int MaxLength) : IRegionalSkillProfileSummaryRule
{
    public List<string> Check(RegionalSkillProfileSummary item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileSummaryCodePrefixRule(string Prefix) : IRegionalSkillProfileSummaryRule
{
    public List<string> Check(RegionalSkillProfileSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileSummaryAllowedNameRule(List<string> Allowed) : IRegionalSkillProfileSummaryRule
{
    public List<string> Check(RegionalSkillProfileSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileSummaryAmountRangeRule(double Min, double Max) : IRegionalSkillProfileSummaryRule
{
    public List<string> Check(RegionalSkillProfileSummary item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileSummaryNameLengthRule(int MaxLength) : IRegionalSkillProfileSummaryRule
{
    public List<string> Check(RegionalSkillProfileSummary item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileSummaryRequiredNameRule(string Message) : IRegionalSkillProfileSummaryRule
{
    public List<string> Check(RegionalSkillProfileSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional skill profile summary.</summary>
public sealed record RegionalSkillProfileSummaryValidator(List<IRegionalSkillProfileSummaryRule> Rules)
{
    public List<string> Validate(RegionalSkillProfileSummary item)
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

    public void RequireValid(RegionalSkillProfileSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSkillProfileSummary> items)
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

/// <summary>Functions of regional skill profile summary rules.</summary>
public static class RegionalSkillProfileSummaryRules
{
    public static RegionalSkillProfileSummaryValidator DefaultRegionalSkillProfileSummaryValidator()
    {
        return new RegionalSkillProfileSummaryValidator(
            new List<IRegionalSkillProfileSummaryRule>
            {
                new RegionalSkillProfileSummaryPhoneLengthRule(86400),
                new RegionalSkillProfileSummaryCodePrefixRule("X"),
                new RegionalSkillProfileSummaryAllowedNameRule(new List<string> { "pending", "manual" }),
                new RegionalSkillProfileSummaryAmountRangeRule(9.0, 3708.1),
                new RegionalSkillProfileSummaryNameLengthRule(86400),
                new RegionalSkillProfileSummaryRequiredNameRule("regional skill profile summary needs an entry"),
            });
    }
}
