namespace Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>A rule a legacy pay band agreement must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayBandAgreementRule
{
    List<string> Check(LegacyPayBandAgreement item);
}

public sealed record LegacyPayBandAgreementCodePrefixRule(string Prefix) : ILegacyPayBandAgreementRule
{
    public List<string> Check(LegacyPayBandAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandAgreementLabelLengthRule(int MaxLength) : ILegacyPayBandAgreementRule
{
    public List<string> Check(LegacyPayBandAgreement item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandAgreementTimezoneLengthRule(int MaxLength) : ILegacyPayBandAgreementRule
{
    public List<string> Check(LegacyPayBandAgreement item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay band agreement.</summary>
public sealed record LegacyPayBandAgreementValidator(List<ILegacyPayBandAgreementRule> Rules)
{
    public List<string> Validate(LegacyPayBandAgreement item)
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

    public void RequireValid(LegacyPayBandAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayBandAgreement> items)
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

/// <summary>Functions of legacy pay band agreement rules.</summary>
public static class LegacyPayBandAgreementRules
{
    public static LegacyPayBandAgreementValidator DefaultLegacyPayBandAgreementValidator()
    {
        return new LegacyPayBandAgreementValidator(
            new List<ILegacyPayBandAgreementRule>
            {
                new LegacyPayBandAgreementCodePrefixRule("OPS"),
                new LegacyPayBandAgreementLabelLengthRule(80),
                new LegacyPayBandAgreementTimezoneLengthRule(255),
            });
    }
}
