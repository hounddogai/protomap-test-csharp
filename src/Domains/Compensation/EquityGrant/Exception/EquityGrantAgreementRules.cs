namespace Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>A rule a equity grant agreement must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantAgreementRule
{
    List<string> Check(EquityGrantAgreement item);
}

public sealed record EquityGrantAgreementLocationLengthRule(int MaxLength) : IEquityGrantAgreementRule
{
    public List<string> Check(EquityGrantAgreement item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAgreementCodeLengthRule(int MaxLength) : IEquityGrantAgreementRule
{
    public List<string> Check(EquityGrantAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAgreementLeaveDaysRangeRule(int Min, int Max) : IEquityGrantAgreementRule
{
    public List<string> Check(EquityGrantAgreement item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant agreement.</summary>
public sealed record EquityGrantAgreementValidator(List<IEquityGrantAgreementRule> Rules)
{
    public List<string> Validate(EquityGrantAgreement item)
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

    public void RequireValid(EquityGrantAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantAgreement> items)
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

/// <summary>Functions of equity grant agreement rules.</summary>
public static class EquityGrantAgreementRules
{
    public static EquityGrantAgreementValidator DefaultEquityGrantAgreementValidator()
    {
        return new EquityGrantAgreementValidator(
            new List<IEquityGrantAgreementRule>
            {
                new EquityGrantAgreementLocationLengthRule(120),
                new EquityGrantAgreementCodeLengthRule(1000),
                new EquityGrantAgreementLeaveDaysRangeRule(64, 255),
            });
    }
}
