namespace Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>A rule a regional skill profile record must pass. Check returns its problems, or none.</summary>
public interface IRegionalSkillProfileRecordRule
{
    List<string> Check(RegionalSkillProfileRecord item);
}

public sealed record RegionalSkillProfileRecordFloorValueRangeRule(double Min, double Max) : IRegionalSkillProfileRecordRule
{
    public List<string> Check(RegionalSkillProfileRecord item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalSkillProfileRecordWindowDaysRangeRule(int Min, int Max) : IRegionalSkillProfileRecordRule
{
    public List<string> Check(RegionalSkillProfileRecord item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileRecordRequiredChecksumRule(string Message) : IRegionalSkillProfileRecordRule
{
    public List<string> Check(RegionalSkillProfileRecord item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileRecordLabelLengthRule(int MaxLength) : IRegionalSkillProfileRecordRule
{
    public List<string> Check(RegionalSkillProfileRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSkillProfileRecordCodePrefixRule(string Prefix) : IRegionalSkillProfileRecordRule
{
    public List<string> Check(RegionalSkillProfileRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional skill profile record.</summary>
public sealed record RegionalSkillProfileRecordValidator(List<IRegionalSkillProfileRecordRule> Rules)
{
    public List<string> Validate(RegionalSkillProfileRecord item)
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

    public void RequireValid(RegionalSkillProfileRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSkillProfileRecord> items)
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

/// <summary>Functions of regional skill profile record rules.</summary>
public static class RegionalSkillProfileRecordRules
{
    public static RegionalSkillProfileRecordValidator DefaultRegionalSkillProfileRecordValidator()
    {
        return new RegionalSkillProfileRecordValidator(
            new List<IRegionalSkillProfileRecordRule>
            {
                new RegionalSkillProfileRecordFloorValueRangeRule(1.3, 1502.8),
                new RegionalSkillProfileRecordWindowDaysRangeRule(1000, 3600),
                new RegionalSkillProfileRecordRequiredChecksumRule("regional skill profile record needs an entry"),
                new RegionalSkillProfileRecordLabelLengthRule(3600),
                new RegionalSkillProfileRecordCodePrefixRule("REF"),
            });
    }
}
