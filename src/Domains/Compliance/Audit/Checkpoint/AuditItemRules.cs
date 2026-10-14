namespace Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>A rule a audit item must pass. Check returns its problems, or none.</summary>
public interface IAuditItemRule
{
    List<string> Check(AuditItem item);
}

public sealed record AuditItemLabelLengthRule(int MaxLength) : IAuditItemRule
{
    public List<string> Check(AuditItem item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditItemRequiredTitleRule(string Message) : IAuditItemRule
{
    public List<string> Check(AuditItem item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditItemThresholdRangeRule(double Min, double Max) : IAuditItemRule
{
    public List<string> Check(AuditItem item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditItemCodePrefixRule(string Prefix) : IAuditItemRule
{
    public List<string> Check(AuditItem item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditItemCodeLengthRule(int MaxLength) : IAuditItemRule
{
    public List<string> Check(AuditItem item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit item.</summary>
public sealed record AuditItemValidator(List<IAuditItemRule> Rules)
{
    public List<string> Validate(AuditItem item)
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

    public void RequireValid(AuditItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditItem> items)
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

/// <summary>Functions of audit item rules.</summary>
public static class AuditItemRules
{
    public static AuditItemValidator DefaultAuditItemValidator()
    {
        return new AuditItemValidator(
            new List<IAuditItemRule>
            {
                new AuditItemLabelLengthRule(255),
                new AuditItemRequiredTitleRule("audit item needs a value"),
                new AuditItemThresholdRangeRule(2.2, 1172.5),
                new AuditItemCodePrefixRule("ACM"),
                new AuditItemCodeLengthRule(3600),
            });
    }
}
