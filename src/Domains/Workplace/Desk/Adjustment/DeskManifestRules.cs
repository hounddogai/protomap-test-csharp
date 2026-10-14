namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk manifest must pass. Check returns its problems, or none.</summary>
public interface IDeskManifestRule
{
    List<string> Check(DeskManifest item);
}

public sealed record DeskManifestRequiredNameRule(string Message) : IDeskManifestRule
{
    public List<string> Check(DeskManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskManifestCodePrefixRule(string Prefix) : IDeskManifestRule
{
    public List<string> Check(DeskManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskManifestAllowedUnitRule(List<string> Allowed) : IDeskManifestRule
{
    public List<string> Check(DeskManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record DeskManifestRevisionRangeRule(int Min, int Max) : IDeskManifestRule
{
    public List<string> Check(DeskManifest item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskManifestNameLengthRule(int MaxLength) : IDeskManifestRule
{
    public List<string> Check(DeskManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk manifest.</summary>
public sealed record DeskManifestValidator(List<IDeskManifestRule> Rules)
{
    public List<string> Validate(DeskManifest item)
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

    public void RequireValid(DeskManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskManifest> items)
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

/// <summary>Functions of desk manifest rules.</summary>
public static class DeskManifestRules
{
    public static DeskManifestValidator DefaultDeskManifestValidator()
    {
        return new DeskManifestValidator(
            new List<IDeskManifestRule>
            {
                new DeskManifestRequiredNameRule("desk manifest needs a value"),
                new DeskManifestCodePrefixRule("OPS"),
                new DeskManifestAllowedUnitRule(new List<string>()),
                new DeskManifestRevisionRangeRule(86400, 86400),
                new DeskManifestNameLengthRule(80),
            });
    }
}
