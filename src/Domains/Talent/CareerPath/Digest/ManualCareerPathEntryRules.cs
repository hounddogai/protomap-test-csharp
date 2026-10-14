namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual career path entry must pass. Check returns its problems, or none.</summary>
public interface IManualCareerPathEntryRule
{
    List<string> Check(ManualCareerPathEntry item);
}

public sealed record ManualCareerPathEntryRequiredLabelRule(string Message) : IManualCareerPathEntryRule
{
    public List<string> Check(ManualCareerPathEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathEntryAllowedSkillRule(List<string> Allowed) : IManualCareerPathEntryRule
{
    public List<string> Check(ManualCareerPathEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is expired" };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathEntryRequiredCodeRule(string Message) : IManualCareerPathEntryRule
{
    public List<string> Check(ManualCareerPathEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathEntryCodePrefixRule(string Prefix) : IManualCareerPathEntryRule
{
    public List<string> Check(ManualCareerPathEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathEntryLabelLengthRule(int MaxLength) : IManualCareerPathEntryRule
{
    public List<string> Check(ManualCareerPathEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathEntryMaximumRangeRule(double Min, double Max) : IManualCareerPathEntryRule
{
    public List<string> Check(ManualCareerPathEntry item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual career path entry.</summary>
public sealed record ManualCareerPathEntryValidator(List<IManualCareerPathEntryRule> Rules)
{
    public List<string> Validate(ManualCareerPathEntry item)
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

    public void RequireValid(ManualCareerPathEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualCareerPathEntry> items)
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

/// <summary>Functions of manual career path entry rules.</summary>
public static class ManualCareerPathEntryRules
{
    public static ManualCareerPathEntryValidator DefaultManualCareerPathEntryValidator()
    {
        return new ManualCareerPathEntryValidator(
            new List<IManualCareerPathEntryRule>
            {
                new ManualCareerPathEntryRequiredLabelRule("manual career path entry needs a value"),
                new ManualCareerPathEntryAllowedSkillRule(new List<string> { "lenient", "seasonal" }),
                new ManualCareerPathEntryRequiredCodeRule("manual career path entry needs a value"),
                new ManualCareerPathEntryCodePrefixRule("Q"),
                new ManualCareerPathEntryLabelLengthRule(1000),
                new ManualCareerPathEntryMaximumRangeRule(6.0, 2179.3),
            });
    }
}
