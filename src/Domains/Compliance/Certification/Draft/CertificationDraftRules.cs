namespace Acme.Workforce.Compliance.Certification.Draft;

/// <summary>A rule a certification draft must pass. Check returns its problems, or none.</summary>
public interface ICertificationDraftRule
{
    List<string> Check(CertificationDraft item);
}

public sealed record CertificationDraftLabelLengthRule(int MaxLength) : ICertificationDraftRule
{
    public List<string> Check(CertificationDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationDraftRequiredTitleRule(string Message) : ICertificationDraftRule
{
    public List<string> Check(CertificationDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationDraftAmountRangeRule(double Min, double Max) : ICertificationDraftRule
{
    public List<string> Check(CertificationDraft item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationDraftRequiredGradeRule(string Message) : ICertificationDraftRule
{
    public List<string> Check(CertificationDraft item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationDraftTitleLengthRule(int MaxLength) : ICertificationDraftRule
{
    public List<string> Check(CertificationDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationDraftCodePrefixRule(string Prefix) : ICertificationDraftRule
{
    public List<string> Check(CertificationDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification draft.</summary>
public sealed record CertificationDraftValidator(List<ICertificationDraftRule> Rules)
{
    public List<string> Validate(CertificationDraft item)
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

    public void RequireValid(CertificationDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationDraft> items)
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

/// <summary>Functions of certification draft rules.</summary>
public static class CertificationDraftRules
{
    public static CertificationDraftValidator DefaultCertificationDraftValidator()
    {
        return new CertificationDraftValidator(
            new List<ICertificationDraftRule>
            {
                new CertificationDraftLabelLengthRule(1000),
                new CertificationDraftRequiredTitleRule("certification draft needs this field"),
                new CertificationDraftAmountRangeRule(1.3, 2373.4),
                new CertificationDraftRequiredGradeRule("certification draft needs a value"),
                new CertificationDraftTitleLengthRule(64),
                new CertificationDraftCodePrefixRule("OPS"),
            });
    }
}
