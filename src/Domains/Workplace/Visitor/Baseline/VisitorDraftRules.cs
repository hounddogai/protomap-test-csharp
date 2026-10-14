namespace Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>A rule a visitor draft must pass. Check returns its problems, or none.</summary>
public interface IVisitorDraftRule
{
    List<string> Check(VisitorDraft item);
}

public sealed record VisitorDraftRequiredCodeRule(string Message) : IVisitorDraftRule
{
    public List<string> Check(VisitorDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorDraftRequiredTitleRule(string Message) : IVisitorDraftRule
{
    public List<string> Check(VisitorDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorDraftCodePrefixRule(string Prefix) : IVisitorDraftRule
{
    public List<string> Check(VisitorDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor draft.</summary>
public sealed record VisitorDraftValidator(List<IVisitorDraftRule> Rules)
{
    public List<string> Validate(VisitorDraft item)
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

    public void RequireValid(VisitorDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorDraft> items)
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

/// <summary>Functions of visitor draft rules.</summary>
public static class VisitorDraftRules
{
    public static VisitorDraftValidator DefaultVisitorDraftValidator()
    {
        return new VisitorDraftValidator(
            new List<IVisitorDraftRule>
            {
                new VisitorDraftRequiredCodeRule("visitor draft needs this field"),
                new VisitorDraftRequiredTitleRule("visitor draft needs a value"),
                new VisitorDraftCodePrefixRule("OPS"),
            });
    }
}
