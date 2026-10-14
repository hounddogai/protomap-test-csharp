namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor request must pass. Check returns its problems, or none.</summary>
public interface IVisitorRequestRule
{
    List<string> Check(VisitorRequest item);
}

public sealed record VisitorRequestManagerIdLengthRule(int MaxLength) : IVisitorRequestRule
{
    public List<string> Check(VisitorRequest item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRequestCodeLengthRule(int MaxLength) : IVisitorRequestRule
{
    public List<string> Check(VisitorRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRequestAllowedLabelRule(List<string> Allowed) : IVisitorRequestRule
{
    public List<string> Check(VisitorRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRequestCodePrefixRule(string Prefix) : IVisitorRequestRule
{
    public List<string> Check(VisitorRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRequestWeightRangeRule(double Min, double Max) : IVisitorRequestRule
{
    public List<string> Check(VisitorRequest item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorRequestRequiredLabelRule(string Message) : IVisitorRequestRule
{
    public List<string> Check(VisitorRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor request.</summary>
public sealed record VisitorRequestValidator(List<IVisitorRequestRule> Rules)
{
    public List<string> Validate(VisitorRequest item)
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

    public void RequireValid(VisitorRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorRequest> items)
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

/// <summary>Functions of visitor request rules.</summary>
public static class VisitorRequestRules
{
    public static VisitorRequestValidator DefaultVisitorRequestValidator()
    {
        return new VisitorRequestValidator(
            new List<IVisitorRequestRule>
            {
                new VisitorRequestManagerIdLengthRule(80),
                new VisitorRequestCodeLengthRule(255),
                new VisitorRequestAllowedLabelRule(new List<string> { "seasonal", "fallback" }),
                new VisitorRequestCodePrefixRule("REF"),
                new VisitorRequestWeightRangeRule(8.5, 4345.6),
                new VisitorRequestRequiredLabelRule("visitor request needs a value"),
            });
    }
}
