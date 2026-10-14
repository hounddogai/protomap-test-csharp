namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit mapping must pass. Check returns its problems, or none.</summary>
public interface IAuditMappingRule
{
    List<string> Check(AuditMapping item);
}

public sealed record AuditMappingCodeLengthRule(int MaxLength) : IAuditMappingRule
{
    public List<string> Check(AuditMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditMappingAllowedCurrencyRule(List<string> Allowed) : IAuditMappingRule
{
    public List<string> Check(AuditMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AuditMappingNameLengthRule(int MaxLength) : IAuditMappingRule
{
    public List<string> Check(AuditMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditMappingTimezoneLengthRule(int MaxLength) : IAuditMappingRule
{
    public List<string> Check(AuditMapping item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditMappingCodePrefixRule(string Prefix) : IAuditMappingRule
{
    public List<string> Check(AuditMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditMappingGraceDaysRangeRule(int Min, int Max) : IAuditMappingRule
{
    public List<string> Check(AuditMapping item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a audit mapping.</summary>
public sealed record AuditMappingValidator(List<IAuditMappingRule> Rules)
{
    public List<string> Validate(AuditMapping item)
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

    public void RequireValid(AuditMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditMapping> items)
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

/// <summary>Functions of audit mapping rules.</summary>
public static class AuditMappingRules
{
    public static AuditMappingValidator DefaultAuditMappingValidator()
    {
        return new AuditMappingValidator(
            new List<IAuditMappingRule>
            {
                new AuditMappingCodeLengthRule(64),
                new AuditMappingAllowedCurrencyRule(new List<string> { "regional" }),
                new AuditMappingNameLengthRule(64),
                new AuditMappingTimezoneLengthRule(32),
                new AuditMappingCodePrefixRule("OPS"),
                new AuditMappingGraceDaysRangeRule(86400, 86400),
            });
    }
}
