namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment agreement must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentAgreementRule
{
    List<string> Check(GarnishmentAgreement item);
}

public sealed record GarnishmentAgreementAllowedCodeRule(List<string> Allowed) : IGarnishmentAgreementRule
{
    public List<string> Check(GarnishmentAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAgreementNameLengthRule(int MaxLength) : IGarnishmentAgreementRule
{
    public List<string> Check(GarnishmentAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAgreementFloorValueRangeRule(double Min, double Max) : IGarnishmentAgreementRule
{
    public List<string> Check(GarnishmentAgreement item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentAgreementRequiredTitleRule(string Message) : IGarnishmentAgreementRule
{
    public List<string> Check(GarnishmentAgreement item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAgreementCodeLengthRule(int MaxLength) : IGarnishmentAgreementRule
{
    public List<string> Check(GarnishmentAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAgreementApprovedGuardRule(double Limit) : IGarnishmentAgreementRule
{
    public List<string> Check(GarnishmentAgreement item)
    {
        if (item.Approved && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment agreement.</summary>
public sealed record GarnishmentAgreementValidator(List<IGarnishmentAgreementRule> Rules)
{
    public List<string> Validate(GarnishmentAgreement item)
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

    public void RequireValid(GarnishmentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentAgreement> items)
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

/// <summary>Functions of garnishment agreement rules.</summary>
public static class GarnishmentAgreementRules
{
    public static GarnishmentAgreementValidator DefaultGarnishmentAgreementValidator()
    {
        return new GarnishmentAgreementValidator(
            new List<IGarnishmentAgreementRule>
            {
                new GarnishmentAgreementAllowedCodeRule(new List<string> { "manual" }),
                new GarnishmentAgreementNameLengthRule(120),
                new GarnishmentAgreementFloorValueRangeRule(9.2, 2516.5),
                new GarnishmentAgreementRequiredTitleRule("garnishment agreement needs a value"),
                new GarnishmentAgreementCodeLengthRule(16),
                new GarnishmentAgreementApprovedGuardRule(4.2),
            });
    }
}
