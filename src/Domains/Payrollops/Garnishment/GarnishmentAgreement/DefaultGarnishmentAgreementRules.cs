namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default garnishment agreement must pass. Check returns its problems, or none.</summary>
public interface IDefaultGarnishmentAgreementRule
{
    List<string> Check(DefaultGarnishmentAgreement item);
}

public sealed record DefaultGarnishmentAgreementAllowedCodeRule(List<string> Allowed) : IDefaultGarnishmentAgreementRule
{
    public List<string> Check(DefaultGarnishmentAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record DefaultGarnishmentAgreementRatioRangeRule(double Min, double Max) : IDefaultGarnishmentAgreementRule
{
    public List<string> Check(DefaultGarnishmentAgreement item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultGarnishmentAgreementDurationMinutesRangeRule(int Min, int Max) : IDefaultGarnishmentAgreementRule
{
    public List<string> Check(DefaultGarnishmentAgreement item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultGarnishmentAgreementCountryLengthRule(int MaxLength) : IDefaultGarnishmentAgreementRule
{
    public List<string> Check(DefaultGarnishmentAgreement item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultGarnishmentAgreementTitleLengthRule(int MaxLength) : IDefaultGarnishmentAgreementRule
{
    public List<string> Check(DefaultGarnishmentAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default garnishment agreement.</summary>
public sealed record DefaultGarnishmentAgreementValidator(List<IDefaultGarnishmentAgreementRule> Rules)
{
    public List<string> Validate(DefaultGarnishmentAgreement item)
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

    public void RequireValid(DefaultGarnishmentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultGarnishmentAgreement> items)
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

/// <summary>Functions of default garnishment agreement rules.</summary>
public static class DefaultGarnishmentAgreementRules
{
    public static DefaultGarnishmentAgreementValidator DefaultDefaultGarnishmentAgreementValidator()
    {
        return new DefaultGarnishmentAgreementValidator(
            new List<IDefaultGarnishmentAgreementRule>
            {
                new DefaultGarnishmentAgreementAllowedCodeRule(new List<string> { "legacy", "manual", "adaptive" }),
                new DefaultGarnishmentAgreementRatioRangeRule(9.9, 2331.2),
                new DefaultGarnishmentAgreementDurationMinutesRangeRule(32, 120),
                new DefaultGarnishmentAgreementCountryLengthRule(64),
                new DefaultGarnishmentAgreementTitleLengthRule(80),
            });
    }
}
