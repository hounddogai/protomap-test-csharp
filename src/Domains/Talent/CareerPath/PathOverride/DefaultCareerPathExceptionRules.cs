namespace Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>A rule a default career path exception must pass. Check returns its problems, or none.</summary>
public interface IDefaultCareerPathExceptionRule
{
    List<string> Check(DefaultCareerPathException item);
}

public sealed record DefaultCareerPathExceptionSequenceRangeRule(int Min, int Max) : IDefaultCareerPathExceptionRule
{
    public List<string> Check(DefaultCareerPathException item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultCareerPathExceptionRequiredCodeRule(string Message) : IDefaultCareerPathExceptionRule
{
    public List<string> Check(DefaultCareerPathException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultCareerPathExceptionCodePrefixRule(string Prefix) : IDefaultCareerPathExceptionRule
{
    public List<string> Check(DefaultCareerPathException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultCareerPathExceptionSegmentLengthRule(int MaxLength) : IDefaultCareerPathExceptionRule
{
    public List<string> Check(DefaultCareerPathException item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultCareerPathExceptionRequiredLabelRule(string Message) : IDefaultCareerPathExceptionRule
{
    public List<string> Check(DefaultCareerPathException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default career path exception.</summary>
public sealed record DefaultCareerPathExceptionValidator(List<IDefaultCareerPathExceptionRule> Rules)
{
    public List<string> Validate(DefaultCareerPathException item)
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

    public void RequireValid(DefaultCareerPathException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultCareerPathException> items)
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

/// <summary>Functions of default career path exception rules.</summary>
public static class DefaultCareerPathExceptionRules
{
    public static DefaultCareerPathExceptionValidator DefaultDefaultCareerPathExceptionValidator()
    {
        return new DefaultCareerPathExceptionValidator(
            new List<IDefaultCareerPathExceptionRule>
            {
                new DefaultCareerPathExceptionSequenceRangeRule(16, 80),
                new DefaultCareerPathExceptionRequiredCodeRule("default career path exception needs a value"),
                new DefaultCareerPathExceptionCodePrefixRule("REF"),
                new DefaultCareerPathExceptionSegmentLengthRule(80),
                new DefaultCareerPathExceptionRequiredLabelRule("default career path exception needs an entry"),
            });
    }
}
