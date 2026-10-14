namespace Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>A rule a capped audit reservation must pass. Check returns its problems, or none.</summary>
public interface ICappedAuditReservationRule
{
    List<string> Check(CappedAuditReservation item);
}

public sealed record CappedAuditReservationChannelLengthRule(int MaxLength) : ICappedAuditReservationRule
{
    public List<string> Check(CappedAuditReservation item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAuditReservationRequiredChannelRule(string Message) : ICappedAuditReservationRule
{
    public List<string> Check(CappedAuditReservation item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAuditReservationRequiredCodeRule(string Message) : ICappedAuditReservationRule
{
    public List<string> Check(CappedAuditReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAuditReservationNameLengthRule(int MaxLength) : ICappedAuditReservationRule
{
    public List<string> Check(CappedAuditReservation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAuditReservationFteRangeRule(double Min, double Max) : ICappedAuditReservationRule
{
    public List<string> Check(CappedAuditReservation item)
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

public sealed record CappedAuditReservationCapacityRangeRule(int Min, int Max) : ICappedAuditReservationRule
{
    public List<string> Check(CappedAuditReservation item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped audit reservation.</summary>
public sealed record CappedAuditReservationValidator(List<ICappedAuditReservationRule> Rules)
{
    public List<string> Validate(CappedAuditReservation item)
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

    public void RequireValid(CappedAuditReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAuditReservation> items)
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

/// <summary>Functions of capped audit reservation rules.</summary>
public static class CappedAuditReservationRules
{
    public static CappedAuditReservationValidator DefaultCappedAuditReservationValidator()
    {
        return new CappedAuditReservationValidator(
            new List<ICappedAuditReservationRule>
            {
                new CappedAuditReservationChannelLengthRule(255),
                new CappedAuditReservationRequiredChannelRule("capped audit reservation needs a value"),
                new CappedAuditReservationRequiredCodeRule("capped audit reservation needs an entry"),
                new CappedAuditReservationNameLengthRule(3600),
                new CappedAuditReservationFteRangeRule(1.1, 3925.3),
                new CappedAuditReservationCapacityRangeRule(16, 255),
            });
    }
}
