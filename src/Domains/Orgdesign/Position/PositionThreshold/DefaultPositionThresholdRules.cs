namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>A rule a default position threshold must pass. Check returns its problems, or none.</summary>
public interface IDefaultPositionThresholdRule
{
    List<string> Check(DefaultPositionThreshold item);
}

public sealed record DefaultPositionThresholdCodePrefixRule(string Prefix) : IDefaultPositionThresholdRule
{
    public List<string> Check(DefaultPositionThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPositionThresholdRequiredTitleRule(string Message) : IDefaultPositionThresholdRule
{
    public List<string> Check(DefaultPositionThreshold item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPositionThresholdCodeLengthRule(int MaxLength) : IDefaultPositionThresholdRule
{
    public List<string> Check(DefaultPositionThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default position threshold.</summary>
public sealed record DefaultPositionThresholdValidator(List<IDefaultPositionThresholdRule> Rules)
{
    public List<string> Validate(DefaultPositionThreshold item)
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

    public void RequireValid(DefaultPositionThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPositionThreshold> items)
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

/// <summary>Functions of default position threshold rules.</summary>
public static class DefaultPositionThresholdRules
{
    public static DefaultPositionThresholdValidator DefaultDefaultPositionThresholdValidator()
    {
        return new DefaultPositionThresholdValidator(
            new List<IDefaultPositionThresholdRule>
            {
                new DefaultPositionThresholdCodePrefixRule("OPS"),
                new DefaultPositionThresholdRequiredTitleRule("default position threshold needs this field"),
                new DefaultPositionThresholdCodeLengthRule(86400),
            });
    }
}
