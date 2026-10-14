namespace Acme.Workforce.Orgdesign.Position.Manifest;

/// <summary>A rule a position manifest must pass. Check returns its problems, or none.</summary>
public interface IPositionManifestRule
{
    List<string> Check(PositionManifest item);
}

public sealed record PositionManifestWindowDaysRangeRule(int Min, int Max) : IPositionManifestRule
{
    public List<string> Check(PositionManifest item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionManifestCodePrefixRule(string Prefix) : IPositionManifestRule
{
    public List<string> Check(PositionManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionManifestManagerIdLengthRule(int MaxLength) : IPositionManifestRule
{
    public List<string> Check(PositionManifest item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position manifest.</summary>
public sealed record PositionManifestValidator(List<IPositionManifestRule> Rules)
{
    public List<string> Validate(PositionManifest item)
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

    public void RequireValid(PositionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionManifest> items)
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

/// <summary>Functions of position manifest rules.</summary>
public static class PositionManifestRules
{
    public static PositionManifestValidator DefaultPositionManifestValidator()
    {
        return new PositionManifestValidator(
            new List<IPositionManifestRule>
            {
                new PositionManifestWindowDaysRangeRule(120, 255),
                new PositionManifestCodePrefixRule("X"),
                new PositionManifestManagerIdLengthRule(86400),
            });
    }
}
