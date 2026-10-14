namespace Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>A rule a lenient pay run agreement must pass. Check returns its problems, or none.</summary>
public interface ILenientPayRunAgreementRule
{
    List<string> Check(LenientPayRunAgreement item);
}

public sealed record LenientPayRunAgreementRequiredCodeRule(string Message) : ILenientPayRunAgreementRule
{
    public List<string> Check(LenientPayRunAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunAgreementRequiredDescriptionRule(string Message) : ILenientPayRunAgreementRule
{
    public List<string> Check(LenientPayRunAgreement item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunAgreementCodeLengthRule(int MaxLength) : ILenientPayRunAgreementRule
{
    public List<string> Check(LenientPayRunAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunAgreementCodePrefixRule(string Prefix) : ILenientPayRunAgreementRule
{
    public List<string> Check(LenientPayRunAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayRunAgreementRequiredBadgeRule(string Message) : ILenientPayRunAgreementRule
{
    public List<string> Check(LenientPayRunAgreement item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient pay run agreement.</summary>
public sealed record LenientPayRunAgreementValidator(List<ILenientPayRunAgreementRule> Rules)
{
    public List<string> Validate(LenientPayRunAgreement item)
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

    public void RequireValid(LenientPayRunAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPayRunAgreement> items)
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

/// <summary>Functions of lenient pay run agreement rules.</summary>
public static class LenientPayRunAgreementRules
{
    public static LenientPayRunAgreementValidator DefaultLenientPayRunAgreementValidator()
    {
        return new LenientPayRunAgreementValidator(
            new List<ILenientPayRunAgreementRule>
            {
                new LenientPayRunAgreementRequiredCodeRule("lenient pay run agreement needs a value"),
                new LenientPayRunAgreementRequiredDescriptionRule("lenient pay run agreement needs a value"),
                new LenientPayRunAgreementCodeLengthRule(32),
                new LenientPayRunAgreementCodePrefixRule("Q"),
                new LenientPayRunAgreementRequiredBadgeRule("lenient pay run agreement needs an entry"),
            });
    }
}
