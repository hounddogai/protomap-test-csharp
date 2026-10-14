namespace Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>A rule a span agreement must pass. Check returns its problems, or none.</summary>
public interface ISpanAgreementRule
{
    List<string> Check(SpanAgreement item);
}

public sealed record SpanAgreementRequiredCodeRule(string Message) : ISpanAgreementRule
{
    public List<string> Check(SpanAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanAgreementCodePrefixRule(string Prefix) : ISpanAgreementRule
{
    public List<string> Check(SpanAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SpanAgreementCodeLengthRule(int MaxLength) : ISpanAgreementRule
{
    public List<string> Check(SpanAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span agreement.</summary>
public sealed record SpanAgreementValidator(List<ISpanAgreementRule> Rules)
{
    public List<string> Validate(SpanAgreement item)
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

    public void RequireValid(SpanAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanAgreement> items)
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

/// <summary>Functions of span agreement rules.</summary>
public static class SpanAgreementRules
{
    public static SpanAgreementValidator DefaultSpanAgreementValidator()
    {
        return new SpanAgreementValidator(
            new List<ISpanAgreementRule>
            {
                new SpanAgreementRequiredCodeRule("span agreement needs a value"),
                new SpanAgreementCodePrefixRule("ACM"),
                new SpanAgreementCodeLengthRule(64),
            });
    }
}
