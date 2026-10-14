namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>A rule a succession plan manifest must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanManifestRule
{
    List<string> Check(SuccessionPlanManifest item);
}

public sealed record SuccessionPlanManifestNotesLengthRule(int MaxLength) : ISuccessionPlanManifestRule
{
    public List<string> Check(SuccessionPlanManifest item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanManifestLabelLengthRule(int MaxLength) : ISuccessionPlanManifestRule
{
    public List<string> Check(SuccessionPlanManifest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanManifestSegmentLengthRule(int MaxLength) : ISuccessionPlanManifestRule
{
    public List<string> Check(SuccessionPlanManifest item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanManifestCodePrefixRule(string Prefix) : ISuccessionPlanManifestRule
{
    public List<string> Check(SuccessionPlanManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan manifest.</summary>
public sealed record SuccessionPlanManifestValidator(List<ISuccessionPlanManifestRule> Rules)
{
    public List<string> Validate(SuccessionPlanManifest item)
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

    public void RequireValid(SuccessionPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanManifest> items)
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

/// <summary>Functions of succession plan manifest rules.</summary>
public static class SuccessionPlanManifestRules
{
    public static SuccessionPlanManifestValidator DefaultSuccessionPlanManifestValidator()
    {
        return new SuccessionPlanManifestValidator(
            new List<ISuccessionPlanManifestRule>
            {
                new SuccessionPlanManifestNotesLengthRule(1000),
                new SuccessionPlanManifestLabelLengthRule(255),
                new SuccessionPlanManifestSegmentLengthRule(255),
                new SuccessionPlanManifestCodePrefixRule("X"),
            });
    }
}
