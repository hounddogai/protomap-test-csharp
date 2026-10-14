namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>A rule a license seat agreement must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatAgreementRule
{
    List<string> Check(LicenseSeatAgreement item);
}

public sealed record LicenseSeatAgreementMaximumRangeRule(double Min, double Max) : ILicenseSeatAgreementRule
{
    public List<string> Check(LicenseSeatAgreement item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatAgreementNameLengthRule(int MaxLength) : ILicenseSeatAgreementRule
{
    public List<string> Check(LicenseSeatAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAgreementGradeLengthRule(int MaxLength) : ILicenseSeatAgreementRule
{
    public List<string> Check(LicenseSeatAgreement item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAgreementRequiredCodeRule(string Message) : ILicenseSeatAgreementRule
{
    public List<string> Check(LicenseSeatAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAgreementCurrencyLengthRule(int MaxLength) : ILicenseSeatAgreementRule
{
    public List<string> Check(LicenseSeatAgreement item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat agreement.</summary>
public sealed record LicenseSeatAgreementValidator(List<ILicenseSeatAgreementRule> Rules)
{
    public List<string> Validate(LicenseSeatAgreement item)
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

    public void RequireValid(LicenseSeatAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatAgreement> items)
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

/// <summary>Functions of license seat agreement rules.</summary>
public static class LicenseSeatAgreementRules
{
    public static LicenseSeatAgreementValidator DefaultLicenseSeatAgreementValidator()
    {
        return new LicenseSeatAgreementValidator(
            new List<ILicenseSeatAgreementRule>
            {
                new LicenseSeatAgreementMaximumRangeRule(9.2, 4923.9),
                new LicenseSeatAgreementNameLengthRule(16),
                new LicenseSeatAgreementGradeLengthRule(255),
                new LicenseSeatAgreementRequiredCodeRule("license seat agreement needs a value"),
                new LicenseSeatAgreementCurrencyLengthRule(120),
            });
    }
}
