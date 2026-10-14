namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional career path rollup must pass. Check returns its problems, or none.</summary>
public interface IRegionalCareerPathRollupRule
{
    List<string> Check(RegionalCareerPathRollup item);
}

public sealed record RegionalCareerPathRollupAllowedNotesRule(List<string> Allowed) : IRegionalCareerPathRollupRule
{
    public List<string> Check(RegionalCareerPathRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathRollupVersionRangeRule(int Min, int Max) : IRegionalCareerPathRollupRule
{
    public List<string> Check(RegionalCareerPathRollup item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalCareerPathRollupNotesLengthRule(int MaxLength) : IRegionalCareerPathRollupRule
{
    public List<string> Check(RegionalCareerPathRollup item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathRollupCodePrefixRule(string Prefix) : IRegionalCareerPathRollupRule
{
    public List<string> Check(RegionalCareerPathRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathRollupRequiredNotesRule(string Message) : IRegionalCareerPathRollupRule
{
    public List<string> Check(RegionalCareerPathRollup item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional career path rollup.</summary>
public sealed record RegionalCareerPathRollupValidator(List<IRegionalCareerPathRollupRule> Rules)
{
    public List<string> Validate(RegionalCareerPathRollup item)
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

    public void RequireValid(RegionalCareerPathRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalCareerPathRollup> items)
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

/// <summary>Functions of regional career path rollup rules.</summary>
public static class RegionalCareerPathRollupRules
{
    public static RegionalCareerPathRollupValidator DefaultRegionalCareerPathRollupValidator()
    {
        return new RegionalCareerPathRollupValidator(
            new List<IRegionalCareerPathRollupRule>
            {
                new RegionalCareerPathRollupAllowedNotesRule(new List<string> { "regional", "batched", "primary" }),
                new RegionalCareerPathRollupVersionRangeRule(120, 3600),
                new RegionalCareerPathRollupNotesLengthRule(1000),
                new RegionalCareerPathRollupCodePrefixRule("REF"),
                new RegionalCareerPathRollupRequiredNotesRule("regional career path rollup needs an entry"),
            });
    }
}
