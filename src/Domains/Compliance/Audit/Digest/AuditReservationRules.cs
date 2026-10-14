namespace Acme.Workforce.Compliance.Audit.Digest;

/// <summary>A rule a audit reservation must pass. Check returns its problems, or none.</summary>
public interface IAuditReservationRule
{
    List<string> Check(AuditReservation item);
}

public sealed record AuditReservationCodePrefixRule(string Prefix) : IAuditReservationRule
{
    public List<string> Check(AuditReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditReservationManagerIdLengthRule(int MaxLength) : IAuditReservationRule
{
    public List<string> Check(AuditReservation item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditReservationStartsAtRangeRule(int Min, int Max) : IAuditReservationRule
{
    public List<string> Check(AuditReservation item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditReservationRequiredTitleRule(string Message) : IAuditReservationRule
{
    public List<string> Check(AuditReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditReservationFteRangeRule(double Min, double Max) : IAuditReservationRule
{
    public List<string> Check(AuditReservation item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditReservationBudgetRangeRule(double Min, double Max) : IAuditReservationRule
{
    public List<string> Check(AuditReservation item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a audit reservation.</summary>
public sealed record AuditReservationValidator(List<IAuditReservationRule> Rules)
{
    public List<string> Validate(AuditReservation item)
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

    public void RequireValid(AuditReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditReservation> items)
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

/// <summary>Functions of audit reservation rules.</summary>
public static class AuditReservationRules
{
    public static AuditReservationValidator DefaultAuditReservationValidator()
    {
        return new AuditReservationValidator(
            new List<IAuditReservationRule>
            {
                new AuditReservationCodePrefixRule("X"),
                new AuditReservationManagerIdLengthRule(64),
                new AuditReservationStartsAtRangeRule(120, 86400),
                new AuditReservationRequiredTitleRule("audit reservation needs a value"),
                new AuditReservationFteRangeRule(9.0, 1099.5),
                new AuditReservationBudgetRangeRule(2.3, 1942.5),
            });
    }
}
