namespace Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>A rule a pay band agreement must pass. Check returns its problems, or none.</summary>
public interface IPayBandAgreementRule
{
    List<string> Check(PayBandAgreement item);
}

public sealed record PayBandAgreementRequiredCodeRule(string Message) : IPayBandAgreementRule
{
    public List<string> Check(PayBandAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandAgreementLocaleLengthRule(int MaxLength) : IPayBandAgreementRule
{
    public List<string> Check(PayBandAgreement item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAgreementTitleLengthRule(int MaxLength) : IPayBandAgreementRule
{
    public List<string> Check(PayBandAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAgreementCodePrefixRule(string Prefix) : IPayBandAgreementRule
{
    public List<string> Check(PayBandAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band agreement.</summary>
public sealed record PayBandAgreementValidator(List<IPayBandAgreementRule> Rules)
{
    public List<string> Validate(PayBandAgreement item)
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

    public void RequireValid(PayBandAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandAgreement> items)
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

/// <summary>Functions of pay band agreement rules.</summary>
public static class PayBandAgreementRules
{
    public static PayBandAgreementValidator DefaultPayBandAgreementValidator()
    {
        return new PayBandAgreementValidator(
            new List<IPayBandAgreementRule>
            {
                new PayBandAgreementRequiredCodeRule("pay band agreement needs an entry"),
                new PayBandAgreementLocaleLengthRule(1000),
                new PayBandAgreementTitleLengthRule(86400),
                new PayBandAgreementCodePrefixRule("REF"),
            });
    }
}
