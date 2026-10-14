namespace Acme.Workforce.Workplace.Visitor.Group;

/// <summary>A rule a visitor profile must pass. Check returns its problems, or none.</summary>
public interface IVisitorProfileRule
{
    List<string> Check(VisitorProfile item);
}

public sealed record VisitorProfileRequiredTimezoneRule(string Message) : IVisitorProfileRule
{
    public List<string> Check(VisitorProfile item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorProfileLabelLengthRule(int MaxLength) : IVisitorProfileRule
{
    public List<string> Check(VisitorProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorProfileCodePrefixRule(string Prefix) : IVisitorProfileRule
{
    public List<string> Check(VisitorProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorProfileRequiredCodeRule(string Message) : IVisitorProfileRule
{
    public List<string> Check(VisitorProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorProfileCodeLengthRule(int MaxLength) : IVisitorProfileRule
{
    public List<string> Check(VisitorProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorProfileBadgeLengthRule(int MaxLength) : IVisitorProfileRule
{
    public List<string> Check(VisitorProfile item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor profile.</summary>
public sealed record VisitorProfileValidator(List<IVisitorProfileRule> Rules)
{
    public List<string> Validate(VisitorProfile item)
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

    public void RequireValid(VisitorProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorProfile> items)
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

/// <summary>Functions of visitor profile rules.</summary>
public static class VisitorProfileRules
{
    public static VisitorProfileValidator DefaultVisitorProfileValidator()
    {
        return new VisitorProfileValidator(
            new List<IVisitorProfileRule>
            {
                new VisitorProfileRequiredTimezoneRule("visitor profile needs this field"),
                new VisitorProfileLabelLengthRule(86400),
                new VisitorProfileCodePrefixRule("Q"),
                new VisitorProfileRequiredCodeRule("visitor profile needs a value"),
                new VisitorProfileCodeLengthRule(86400),
                new VisitorProfileBadgeLengthRule(64),
            });
    }
}
