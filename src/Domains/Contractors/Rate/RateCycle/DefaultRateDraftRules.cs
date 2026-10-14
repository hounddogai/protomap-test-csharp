namespace Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>A rule a default rate draft must pass. Check returns its problems, or none.</summary>
public interface IDefaultRateDraftRule
{
    List<string> Check(DefaultRateDraft item);
}

public sealed record DefaultRateDraftRequiredNameRule(string Message) : IDefaultRateDraftRule
{
    public List<string> Check(DefaultRateDraft item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRateDraftRequiredSummaryRule(string Message) : IDefaultRateDraftRule
{
    public List<string> Check(DefaultRateDraft item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRateDraftRequiredCodeRule(string Message) : IDefaultRateDraftRule
{
    public List<string> Check(DefaultRateDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default rate draft.</summary>
public sealed record DefaultRateDraftValidator(List<IDefaultRateDraftRule> Rules)
{
    public List<string> Validate(DefaultRateDraft item)
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

    public void RequireValid(DefaultRateDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultRateDraft> items)
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

/// <summary>Functions of default rate draft rules.</summary>
public static class DefaultRateDraftRules
{
    public static DefaultRateDraftValidator DefaultDefaultRateDraftValidator()
    {
        return new DefaultRateDraftValidator(
            new List<IDefaultRateDraftRule>
            {
                new DefaultRateDraftRequiredNameRule("default rate draft needs a value"),
                new DefaultRateDraftRequiredSummaryRule("default rate draft needs an entry"),
                new DefaultRateDraftRequiredCodeRule("default rate draft needs an entry"),
            });
    }
}
