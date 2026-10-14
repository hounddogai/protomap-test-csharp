namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>A rule a garnishment quota must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentQuotaRule
{
    List<string> Check(GarnishmentQuota item);
}

public sealed record GarnishmentQuotaMinimumRangeRule(double Min, double Max) : IGarnishmentQuotaRule
{
    public List<string> Check(GarnishmentQuota item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentQuotaNameLengthRule(int MaxLength) : IGarnishmentQuotaRule
{
    public List<string> Check(GarnishmentQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentQuotaRequiredSourceRule(string Message) : IGarnishmentQuotaRule
{
    public List<string> Check(GarnishmentQuota item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentQuotaDiscountRangeRule(double Min, double Max) : IGarnishmentQuotaRule
{
    public List<string> Check(GarnishmentQuota item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentQuotaCodeLengthRule(int MaxLength) : IGarnishmentQuotaRule
{
    public List<string> Check(GarnishmentQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment quota.</summary>
public sealed record GarnishmentQuotaValidator(List<IGarnishmentQuotaRule> Rules)
{
    public List<string> Validate(GarnishmentQuota item)
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

    public void RequireValid(GarnishmentQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentQuota> items)
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

/// <summary>Functions of garnishment quota rules.</summary>
public static class GarnishmentQuotaRules
{
    public static GarnishmentQuotaValidator DefaultGarnishmentQuotaValidator()
    {
        return new GarnishmentQuotaValidator(
            new List<IGarnishmentQuotaRule>
            {
                new GarnishmentQuotaMinimumRangeRule(2.3, 264.9),
                new GarnishmentQuotaNameLengthRule(32),
                new GarnishmentQuotaRequiredSourceRule("garnishment quota needs a value"),
                new GarnishmentQuotaDiscountRangeRule(7.0, 3628.8),
                new GarnishmentQuotaCodeLengthRule(64),
            });
    }
}
