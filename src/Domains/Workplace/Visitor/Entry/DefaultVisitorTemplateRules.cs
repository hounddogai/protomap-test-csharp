namespace Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>A rule a default visitor template must pass. Check returns its problems, or none.</summary>
public interface IDefaultVisitorTemplateRule
{
    List<string> Check(DefaultVisitorTemplate item);
}

public sealed record DefaultVisitorTemplateNameLengthRule(int MaxLength) : IDefaultVisitorTemplateRule
{
    public List<string> Check(DefaultVisitorTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorTemplateArchivedGuardRule(int Limit) : IDefaultVisitorTemplateRule
{
    public List<string> Check(DefaultVisitorTemplate item)
    {
        if (item.Archived && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorTemplateRequiredCountryRule(string Message) : IDefaultVisitorTemplateRule
{
    public List<string> Check(DefaultVisitorTemplate item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorTemplateCodePrefixRule(string Prefix) : IDefaultVisitorTemplateRule
{
    public List<string> Check(DefaultVisitorTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorTemplateSequenceRangeRule(int Min, int Max) : IDefaultVisitorTemplateRule
{
    public List<string> Check(DefaultVisitorTemplate item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorTemplatePriorityRangeRule(int Min, int Max) : IDefaultVisitorTemplateRule
{
    public List<string> Check(DefaultVisitorTemplate item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default visitor template.</summary>
public sealed record DefaultVisitorTemplateValidator(List<IDefaultVisitorTemplateRule> Rules)
{
    public List<string> Validate(DefaultVisitorTemplate item)
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

    public void RequireValid(DefaultVisitorTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultVisitorTemplate> items)
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

/// <summary>Functions of default visitor template rules.</summary>
public static class DefaultVisitorTemplateRules
{
    public static DefaultVisitorTemplateValidator DefaultDefaultVisitorTemplateValidator()
    {
        return new DefaultVisitorTemplateValidator(
            new List<IDefaultVisitorTemplateRule>
            {
                new DefaultVisitorTemplateNameLengthRule(1000),
                new DefaultVisitorTemplateArchivedGuardRule(80),
                new DefaultVisitorTemplateRequiredCountryRule("default visitor template needs a value"),
                new DefaultVisitorTemplateCodePrefixRule("ACM"),
                new DefaultVisitorTemplateSequenceRangeRule(32, 120),
                new DefaultVisitorTemplatePriorityRangeRule(16, 16),
            });
    }
}
