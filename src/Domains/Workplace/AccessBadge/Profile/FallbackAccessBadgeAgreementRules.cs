namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback access badge agreement must pass. Check returns its problems, or none.</summary>
public interface IFallbackAccessBadgeAgreementRule
{
    List<string> Check(FallbackAccessBadgeAgreement item);
}

public sealed record FallbackAccessBadgeAgreementAllowedNameRule(List<string> Allowed) : IFallbackAccessBadgeAgreementRule
{
    public List<string> Check(FallbackAccessBadgeAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeAgreementMarginRangeRule(double Min, double Max) : IFallbackAccessBadgeAgreementRule
{
    public List<string> Check(FallbackAccessBadgeAgreement item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackAccessBadgeAgreementRequiredCodeRule(string Message) : IFallbackAccessBadgeAgreementRule
{
    public List<string> Check(FallbackAccessBadgeAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeAgreementCodeLengthRule(int MaxLength) : IFallbackAccessBadgeAgreementRule
{
    public List<string> Check(FallbackAccessBadgeAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeAgreementExpiresAtRangeRule(int Min, int Max) : IFallbackAccessBadgeAgreementRule
{
    public List<string> Check(FallbackAccessBadgeAgreement item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback access badge agreement.</summary>
public sealed record FallbackAccessBadgeAgreementValidator(List<IFallbackAccessBadgeAgreementRule> Rules)
{
    public List<string> Validate(FallbackAccessBadgeAgreement item)
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

    public void RequireValid(FallbackAccessBadgeAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAccessBadgeAgreement> items)
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

/// <summary>Functions of fallback access badge agreement rules.</summary>
public static class FallbackAccessBadgeAgreementRules
{
    public static FallbackAccessBadgeAgreementValidator DefaultFallbackAccessBadgeAgreementValidator()
    {
        return new FallbackAccessBadgeAgreementValidator(
            new List<IFallbackAccessBadgeAgreementRule>
            {
                new FallbackAccessBadgeAgreementAllowedNameRule(new List<string> { "pending" }),
                new FallbackAccessBadgeAgreementMarginRangeRule(7.5, 4492.5),
                new FallbackAccessBadgeAgreementRequiredCodeRule("fallback access badge agreement needs a value"),
                new FallbackAccessBadgeAgreementCodeLengthRule(1000),
                new FallbackAccessBadgeAgreementExpiresAtRangeRule(16, 80),
            });
    }
}
