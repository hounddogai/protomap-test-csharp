namespace Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>A rule a pay band draft must pass. Check returns its problems, or none.</summary>
public interface IPayBandDraftRule
{
    List<string> Check(PayBandDraft item);
}

public sealed record PayBandDraftNameLengthRule(int MaxLength) : IPayBandDraftRule
{
    public List<string> Check(PayBandDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandDraftCountryLengthRule(int MaxLength) : IPayBandDraftRule
{
    public List<string> Check(PayBandDraft item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandDraftCodeLengthRule(int MaxLength) : IPayBandDraftRule
{
    public List<string> Check(PayBandDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band draft.</summary>
public sealed record PayBandDraftValidator(List<IPayBandDraftRule> Rules)
{
    public List<string> Validate(PayBandDraft item)
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

    public void RequireValid(PayBandDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandDraft> items)
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

/// <summary>Functions of pay band draft rules.</summary>
public static class PayBandDraftRules
{
    public static PayBandDraftValidator DefaultPayBandDraftValidator()
    {
        return new PayBandDraftValidator(
            new List<IPayBandDraftRule>
            {
                new PayBandDraftNameLengthRule(16),
                new PayBandDraftCountryLengthRule(120),
                new PayBandDraftCodeLengthRule(86400),
            });
    }
}
