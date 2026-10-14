namespace Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>A rule a balance agreement must pass. Check returns its problems, or none.</summary>
public interface IBalanceAgreementRule
{
    List<string> Check(BalanceAgreement item);
}

public sealed record BalanceAgreementLabelLengthRule(int MaxLength) : IBalanceAgreementRule
{
    public List<string> Check(BalanceAgreement item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceAgreementRequiredJobCodeRule(string Message) : IBalanceAgreementRule
{
    public List<string> Check(BalanceAgreement item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceAgreementRequiredLabelRule(string Message) : IBalanceAgreementRule
{
    public List<string> Check(BalanceAgreement item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceAgreementArchivedGuardRule(double Limit) : IBalanceAgreementRule
{
    public List<string> Check(BalanceAgreement item)
    {
        if (item.Archived && item.Ratio > Limit)
        {
            return new List<string> { $"ratio {item.Ratio} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record BalanceAgreementCodePrefixRule(string Prefix) : IBalanceAgreementRule
{
    public List<string> Check(BalanceAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceAgreementRatioRangeRule(double Min, double Max) : IBalanceAgreementRule
{
    public List<string> Check(BalanceAgreement item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance agreement.</summary>
public sealed record BalanceAgreementValidator(List<IBalanceAgreementRule> Rules)
{
    public List<string> Validate(BalanceAgreement item)
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

    public void RequireValid(BalanceAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceAgreement> items)
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

/// <summary>Functions of balance agreement rules.</summary>
public static class BalanceAgreementRules
{
    public static BalanceAgreementValidator DefaultBalanceAgreementValidator()
    {
        return new BalanceAgreementValidator(
            new List<IBalanceAgreementRule>
            {
                new BalanceAgreementLabelLengthRule(255),
                new BalanceAgreementRequiredJobCodeRule("balance agreement needs a value"),
                new BalanceAgreementRequiredLabelRule("balance agreement needs an entry"),
                new BalanceAgreementArchivedGuardRule(9.9),
                new BalanceAgreementCodePrefixRule("ACM"),
                new BalanceAgreementRatioRangeRule(5.5, 4832.3),
            });
    }
}
