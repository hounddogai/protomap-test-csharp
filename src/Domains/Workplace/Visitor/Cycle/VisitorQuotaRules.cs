namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor quota must pass. Check returns its problems, or none.</summary>
public interface IVisitorQuotaRule
{
    List<string> Check(VisitorQuota item);
}

public sealed record VisitorQuotaRequiredCodeRule(string Message) : IVisitorQuotaRule
{
    public List<string> Check(VisitorQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorQuotaLabelLengthRule(int MaxLength) : IVisitorQuotaRule
{
    public List<string> Check(VisitorQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorQuotaCodePrefixRule(string Prefix) : IVisitorQuotaRule
{
    public List<string> Check(VisitorQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorQuotaAllowedSourceRule(List<string> Allowed) : IVisitorQuotaRule
{
    public List<string> Check(VisitorQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor quota.</summary>
public sealed record VisitorQuotaValidator(List<IVisitorQuotaRule> Rules)
{
    public List<string> Validate(VisitorQuota item)
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

    public void RequireValid(VisitorQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorQuota> items)
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

/// <summary>Functions of visitor quota rules.</summary>
public static class VisitorQuotaRules
{
    public static VisitorQuotaValidator DefaultVisitorQuotaValidator()
    {
        return new VisitorQuotaValidator(
            new List<IVisitorQuotaRule>
            {
                new VisitorQuotaRequiredCodeRule("visitor quota needs a value"),
                new VisitorQuotaLabelLengthRule(16),
                new VisitorQuotaCodePrefixRule("REF"),
                new VisitorQuotaAllowedSourceRule(new List<string> { "seasonal" }),
            });
    }
}
