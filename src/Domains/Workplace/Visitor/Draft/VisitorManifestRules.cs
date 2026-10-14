namespace Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>A rule a visitor manifest must pass. Check returns its problems, or none.</summary>
public interface IVisitorManifestRule
{
    List<string> Check(VisitorManifest item);
}

public sealed record VisitorManifestHoursRangeRule(double Min, double Max) : IVisitorManifestRule
{
    public List<string> Check(VisitorManifest item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorManifestCodeLengthRule(int MaxLength) : IVisitorManifestRule
{
    public List<string> Check(VisitorManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorManifestRequiredCodeRule(string Message) : IVisitorManifestRule
{
    public List<string> Check(VisitorManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorManifestCodePrefixRule(string Prefix) : IVisitorManifestRule
{
    public List<string> Check(VisitorManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor manifest.</summary>
public sealed record VisitorManifestValidator(List<IVisitorManifestRule> Rules)
{
    public List<string> Validate(VisitorManifest item)
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

    public void RequireValid(VisitorManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorManifest> items)
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

/// <summary>Functions of visitor manifest rules.</summary>
public static class VisitorManifestRules
{
    public static VisitorManifestValidator DefaultVisitorManifestValidator()
    {
        return new VisitorManifestValidator(
            new List<IVisitorManifestRule>
            {
                new VisitorManifestHoursRangeRule(7.3, 2264.0),
                new VisitorManifestCodeLengthRule(64),
                new VisitorManifestRequiredCodeRule("visitor manifest needs a value"),
                new VisitorManifestCodePrefixRule("OPS"),
            });
    }
}
