namespace Acme.Workforce.Compliance.Audit.Manifest;

/// <summary>A rule a audit estimate must pass. Check returns its problems, or none.</summary>
public interface IAuditEstimateRule
{
    List<string> Check(AuditEstimate item);
}

public sealed record AuditEstimateRequiredCodeRule(string Message) : IAuditEstimateRule
{
    public List<string> Check(AuditEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditEstimateRatingRangeRule(double Min, double Max) : IAuditEstimateRule
{
    public List<string> Check(AuditEstimate item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditEstimateNameLengthRule(int MaxLength) : IAuditEstimateRule
{
    public List<string> Check(AuditEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit estimate.</summary>
public sealed record AuditEstimateValidator(List<IAuditEstimateRule> Rules)
{
    public List<string> Validate(AuditEstimate item)
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

    public void RequireValid(AuditEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditEstimate> items)
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

/// <summary>Functions of audit estimate rules.</summary>
public static class AuditEstimateRules
{
    public static AuditEstimateValidator DefaultAuditEstimateValidator()
    {
        return new AuditEstimateValidator(
            new List<IAuditEstimateRule>
            {
                new AuditEstimateRequiredCodeRule("audit estimate needs a value"),
                new AuditEstimateRatingRangeRule(0.0, 3160.0),
                new AuditEstimateNameLengthRule(32),
            });
    }
}
