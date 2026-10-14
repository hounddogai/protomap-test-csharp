namespace Acme.Workforce.Compliance.Audit.Claim;

/// <summary>A rule a audit profile must pass. Check returns its problems, or none.</summary>
public interface IAuditProfileRule
{
    List<string> Check(AuditProfile item);
}

public sealed record AuditProfileDiscountRangeRule(double Min, double Max) : IAuditProfileRule
{
    public List<string> Check(AuditProfile item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditProfileCodeLengthRule(int MaxLength) : IAuditProfileRule
{
    public List<string> Check(AuditProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditProfileLocaleLengthRule(int MaxLength) : IAuditProfileRule
{
    public List<string> Check(AuditProfile item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditProfileRequiredLocaleRule(string Message) : IAuditProfileRule
{
    public List<string> Check(AuditProfile item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditProfileTenureMonthsRangeRule(int Min, int Max) : IAuditProfileRule
{
    public List<string> Check(AuditProfile item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditProfileLabelLengthRule(int MaxLength) : IAuditProfileRule
{
    public List<string> Check(AuditProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit profile.</summary>
public sealed record AuditProfileValidator(List<IAuditProfileRule> Rules)
{
    public List<string> Validate(AuditProfile item)
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

    public void RequireValid(AuditProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditProfile> items)
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

/// <summary>Functions of audit profile rules.</summary>
public static class AuditProfileRules
{
    public static AuditProfileValidator DefaultAuditProfileValidator()
    {
        return new AuditProfileValidator(
            new List<IAuditProfileRule>
            {
                new AuditProfileDiscountRangeRule(7.1, 2984.8),
                new AuditProfileCodeLengthRule(32),
                new AuditProfileLocaleLengthRule(16),
                new AuditProfileRequiredLocaleRule("audit profile needs a value"),
                new AuditProfileTenureMonthsRangeRule(1000, 3600),
                new AuditProfileLabelLengthRule(255),
            });
    }
}
