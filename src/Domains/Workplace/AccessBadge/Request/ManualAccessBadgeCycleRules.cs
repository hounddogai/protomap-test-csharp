namespace Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>A rule a manual access badge cycle must pass. Check returns its problems, or none.</summary>
public interface IManualAccessBadgeCycleRule
{
    List<string> Check(ManualAccessBadgeCycle item);
}

public sealed record ManualAccessBadgeCycleRequiredLabelRule(string Message) : IManualAccessBadgeCycleRule
{
    public List<string> Check(ManualAccessBadgeCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeCycleCodePrefixRule(string Prefix) : IManualAccessBadgeCycleRule
{
    public List<string> Check(ManualAccessBadgeCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeCycleJobCodeLengthRule(int MaxLength) : IManualAccessBadgeCycleRule
{
    public List<string> Check(ManualAccessBadgeCycle item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeCycleSegmentLengthRule(int MaxLength) : IManualAccessBadgeCycleRule
{
    public List<string> Check(ManualAccessBadgeCycle item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual access badge cycle.</summary>
public sealed record ManualAccessBadgeCycleValidator(List<IManualAccessBadgeCycleRule> Rules)
{
    public List<string> Validate(ManualAccessBadgeCycle item)
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

    public void RequireValid(ManualAccessBadgeCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAccessBadgeCycle> items)
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

/// <summary>Functions of manual access badge cycle rules.</summary>
public static class ManualAccessBadgeCycleRules
{
    public static ManualAccessBadgeCycleValidator DefaultManualAccessBadgeCycleValidator()
    {
        return new ManualAccessBadgeCycleValidator(
            new List<IManualAccessBadgeCycleRule>
            {
                new ManualAccessBadgeCycleRequiredLabelRule("manual access badge cycle needs this field"),
                new ManualAccessBadgeCycleCodePrefixRule("REF"),
                new ManualAccessBadgeCycleJobCodeLengthRule(16),
                new ManualAccessBadgeCycleSegmentLengthRule(1000),
            });
    }
}
