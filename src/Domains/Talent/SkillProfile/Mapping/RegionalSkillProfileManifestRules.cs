namespace Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>A rule a regional skill profile manifest must pass. Check returns its problems, or none.</summary>
public interface IRegionalSkillProfileManifestRule
{
    List<string> Check(RegionalSkillProfileManifest item);
}

public sealed record RegionalSkillProfileManifestRequiredNotesRule(string Message) : IRegionalSkillProfileManifestRule
{
    public List<string> Check(RegionalSkillProfileManifest item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileManifestNotesLengthRule(int MaxLength) : IRegionalSkillProfileManifestRule
{
    public List<string> Check(RegionalSkillProfileManifest item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileManifestRequiredNameRule(string Message) : IRegionalSkillProfileManifestRule
{
    public List<string> Check(RegionalSkillProfileManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileManifestNameLengthRule(int MaxLength) : IRegionalSkillProfileManifestRule
{
    public List<string> Check(RegionalSkillProfileManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileManifestRequiredCodeRule(string Message) : IRegionalSkillProfileManifestRule
{
    public List<string> Check(RegionalSkillProfileManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional skill profile manifest.</summary>
public sealed record RegionalSkillProfileManifestValidator(List<IRegionalSkillProfileManifestRule> Rules)
{
    public List<string> Validate(RegionalSkillProfileManifest item)
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

    public void RequireValid(RegionalSkillProfileManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSkillProfileManifest> items)
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

/// <summary>Functions of regional skill profile manifest rules.</summary>
public static class RegionalSkillProfileManifestRules
{
    public static RegionalSkillProfileManifestValidator DefaultRegionalSkillProfileManifestValidator()
    {
        return new RegionalSkillProfileManifestValidator(
            new List<IRegionalSkillProfileManifestRule>
            {
                new RegionalSkillProfileManifestRequiredNotesRule("regional skill profile manifest needs an entry"),
                new RegionalSkillProfileManifestNotesLengthRule(120),
                new RegionalSkillProfileManifestRequiredNameRule("regional skill profile manifest needs a value"),
                new RegionalSkillProfileManifestNameLengthRule(32),
                new RegionalSkillProfileManifestRequiredCodeRule("regional skill profile manifest needs this field"),
            });
    }
}
