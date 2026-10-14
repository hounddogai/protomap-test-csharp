namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient leave request agreement must pass. Check returns its problems, or none.</summary>
public interface ILenientLeaveRequestAgreementRule
{
    List<string> Check(LenientLeaveRequestAgreement item);
}

public sealed record LenientLeaveRequestAgreementNameLengthRule(int MaxLength) : ILenientLeaveRequestAgreementRule
{
    public List<string> Check(LenientLeaveRequestAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestAgreementLimitRangeRule(int Min, int Max) : ILenientLeaveRequestAgreementRule
{
    public List<string> Check(LenientLeaveRequestAgreement item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestAgreementCodeLengthRule(int MaxLength) : ILenientLeaveRequestAgreementRule
{
    public List<string> Check(LenientLeaveRequestAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestAgreementRequiredChannelRule(string Message) : ILenientLeaveRequestAgreementRule
{
    public List<string> Check(LenientLeaveRequestAgreement item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestAgreementCodePrefixRule(string Prefix) : ILenientLeaveRequestAgreementRule
{
    public List<string> Check(LenientLeaveRequestAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestAgreementAllowedCodeRule(List<string> Allowed) : ILenientLeaveRequestAgreementRule
{
    public List<string> Check(LenientLeaveRequestAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient leave request agreement.</summary>
public sealed record LenientLeaveRequestAgreementValidator(List<ILenientLeaveRequestAgreementRule> Rules)
{
    public List<string> Validate(LenientLeaveRequestAgreement item)
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

    public void RequireValid(LenientLeaveRequestAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLeaveRequestAgreement> items)
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

/// <summary>Functions of lenient leave request agreement rules.</summary>
public static class LenientLeaveRequestAgreementRules
{
    public static LenientLeaveRequestAgreementValidator DefaultLenientLeaveRequestAgreementValidator()
    {
        return new LenientLeaveRequestAgreementValidator(
            new List<ILenientLeaveRequestAgreementRule>
            {
                new LenientLeaveRequestAgreementNameLengthRule(86400),
                new LenientLeaveRequestAgreementLimitRangeRule(3600, 86400),
                new LenientLeaveRequestAgreementCodeLengthRule(255),
                new LenientLeaveRequestAgreementRequiredChannelRule(
                    "lenient leave request agreement needs this field"),
                new LenientLeaveRequestAgreementCodePrefixRule("Q"),
                new LenientLeaveRequestAgreementAllowedCodeRule(new List<string> { "fallback", "capped" }),
            });
    }
}
