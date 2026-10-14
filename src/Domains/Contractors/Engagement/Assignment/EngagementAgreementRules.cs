namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement agreement must pass. Check returns its problems, or none.</summary>
public interface IEngagementAgreementRule
{
    List<string> Check(EngagementAgreement item);
}

public sealed record EngagementAgreementRequiredLabelRule(string Message) : IEngagementAgreementRule
{
    public List<string> Check(EngagementAgreement item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementAgreementRequiredCodeRule(string Message) : IEngagementAgreementRule
{
    public List<string> Check(EngagementAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementAgreementTitleLengthRule(int MaxLength) : IEngagementAgreementRule
{
    public List<string> Check(EngagementAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAgreementAllowedLabelRule(List<string> Allowed) : IEngagementAgreementRule
{
    public List<string> Check(EngagementAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAgreementBatchSizeRangeRule(int Min, int Max) : IEngagementAgreementRule
{
    public List<string> Check(EngagementAgreement item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAgreementCodePrefixRule(string Prefix) : IEngagementAgreementRule
{
    public List<string> Check(EngagementAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement agreement.</summary>
public sealed record EngagementAgreementValidator(List<IEngagementAgreementRule> Rules)
{
    public List<string> Validate(EngagementAgreement item)
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

    public void RequireValid(EngagementAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementAgreement> items)
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

/// <summary>Functions of engagement agreement rules.</summary>
public static class EngagementAgreementRules
{
    public static EngagementAgreementValidator DefaultEngagementAgreementValidator()
    {
        return new EngagementAgreementValidator(
            new List<IEngagementAgreementRule>
            {
                new EngagementAgreementRequiredLabelRule("engagement agreement needs an entry"),
                new EngagementAgreementRequiredCodeRule("engagement agreement needs a value"),
                new EngagementAgreementTitleLengthRule(86400),
                new EngagementAgreementAllowedLabelRule(new List<string> { "pending", "strict" }),
                new EngagementAgreementBatchSizeRangeRule(32, 255),
                new EngagementAgreementCodePrefixRule("OPS"),
            });
    }
}
