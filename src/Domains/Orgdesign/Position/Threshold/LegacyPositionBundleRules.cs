namespace Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>A rule a legacy position bundle must pass. Check returns its problems, or none.</summary>
public interface ILegacyPositionBundleRule
{
    List<string> Check(LegacyPositionBundle item);
}

public sealed record LegacyPositionBundleSegmentLengthRule(int MaxLength) : ILegacyPositionBundleRule
{
    public List<string> Check(LegacyPositionBundle item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionBundleRequiredLabelRule(string Message) : ILegacyPositionBundleRule
{
    public List<string> Check(LegacyPositionBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionBundleContactEmailLengthRule(int MaxLength) : ILegacyPositionBundleRule
{
    public List<string> Check(LegacyPositionBundle item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionBundleLabelLengthRule(int MaxLength) : ILegacyPositionBundleRule
{
    public List<string> Check(LegacyPositionBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy position bundle.</summary>
public sealed record LegacyPositionBundleValidator(List<ILegacyPositionBundleRule> Rules)
{
    public List<string> Validate(LegacyPositionBundle item)
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

    public void RequireValid(LegacyPositionBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPositionBundle> items)
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

/// <summary>Functions of legacy position bundle rules.</summary>
public static class LegacyPositionBundleRules
{
    public static LegacyPositionBundleValidator DefaultLegacyPositionBundleValidator()
    {
        return new LegacyPositionBundleValidator(
            new List<ILegacyPositionBundleRule>
            {
                new LegacyPositionBundleSegmentLengthRule(3600),
                new LegacyPositionBundleRequiredLabelRule("legacy position bundle needs an entry"),
                new LegacyPositionBundleContactEmailLengthRule(3600),
                new LegacyPositionBundleLabelLengthRule(1000),
            });
    }
}
