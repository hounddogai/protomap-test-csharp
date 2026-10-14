namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal audit claim must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAuditClaimRule
{
    List<string> Check(SeasonalAuditClaim item);
}

public sealed record SeasonalAuditClaimAmountRangeRule(double Min, double Max) : ISeasonalAuditClaimRule
{
    public List<string> Check(SeasonalAuditClaim item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditClaimAllowedJobCodeRule(List<string> Allowed) : ISeasonalAuditClaimRule
{
    public List<string> Check(SeasonalAuditClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditClaimVisibleGuardRule(double Limit) : ISeasonalAuditClaimRule
{
    public List<string> Check(SeasonalAuditClaim item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditClaimCountryLengthRule(int MaxLength) : ISeasonalAuditClaimRule
{
    public List<string> Check(SeasonalAuditClaim item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal audit claim.</summary>
public sealed record SeasonalAuditClaimValidator(List<ISeasonalAuditClaimRule> Rules)
{
    public List<string> Validate(SeasonalAuditClaim item)
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

    public void RequireValid(SeasonalAuditClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAuditClaim> items)
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

/// <summary>Functions of seasonal audit claim rules.</summary>
public static class SeasonalAuditClaimRules
{
    public static SeasonalAuditClaimValidator DefaultSeasonalAuditClaimValidator()
    {
        return new SeasonalAuditClaimValidator(
            new List<ISeasonalAuditClaimRule>
            {
                new SeasonalAuditClaimAmountRangeRule(3.0, 1926.0),
                new SeasonalAuditClaimAllowedJobCodeRule(new List<string> { "default" }),
                new SeasonalAuditClaimVisibleGuardRule(3.6),
                new SeasonalAuditClaimCountryLengthRule(64),
            });
    }
}
