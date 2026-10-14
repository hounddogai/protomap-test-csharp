namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>A rule a legacy pay run agreement must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayRunAgreementRule
{
    List<string> Check(LegacyPayRunAgreement item);
}

public sealed record LegacyPayRunAgreementCodeLengthRule(int MaxLength) : ILegacyPayRunAgreementRule
{
    public List<string> Check(LegacyPayRunAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAgreementRequiredReviewerRule(string Message) : ILegacyPayRunAgreementRule
{
    public List<string> Check(LegacyPayRunAgreement item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAgreementRequiredCodeRule(string Message) : ILegacyPayRunAgreementRule
{
    public List<string> Check(LegacyPayRunAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAgreementStartsAtRangeRule(int Min, int Max) : ILegacyPayRunAgreementRule
{
    public List<string> Check(LegacyPayRunAgreement item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAgreementCodePrefixRule(string Prefix) : ILegacyPayRunAgreementRule
{
    public List<string> Check(LegacyPayRunAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayRunAgreementReviewerLengthRule(int MaxLength) : ILegacyPayRunAgreementRule
{
    public List<string> Check(LegacyPayRunAgreement item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay run agreement.</summary>
public sealed record LegacyPayRunAgreementValidator(List<ILegacyPayRunAgreementRule> Rules)
{
    public List<string> Validate(LegacyPayRunAgreement item)
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

    public void RequireValid(LegacyPayRunAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayRunAgreement> items)
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

/// <summary>Functions of legacy pay run agreement rules.</summary>
public static class LegacyPayRunAgreementRules
{
    public static LegacyPayRunAgreementValidator DefaultLegacyPayRunAgreementValidator()
    {
        return new LegacyPayRunAgreementValidator(
            new List<ILegacyPayRunAgreementRule>
            {
                new LegacyPayRunAgreementCodeLengthRule(3600),
                new LegacyPayRunAgreementRequiredReviewerRule("legacy pay run agreement needs a value"),
                new LegacyPayRunAgreementRequiredCodeRule("legacy pay run agreement needs an entry"),
                new LegacyPayRunAgreementStartsAtRangeRule(1000, 3600),
                new LegacyPayRunAgreementCodePrefixRule("OPS"),
                new LegacyPayRunAgreementReviewerLengthRule(255),
            });
    }
}
