namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>A rule a mobility request must pass. Check returns its problems, or none.</summary>
public interface IMobilityRequestRule
{
    List<string> Check(MobilityRequest item);
}

public sealed record MobilityRequestRequiredLabelRule(string Message) : IMobilityRequestRule
{
    public List<string> Check(MobilityRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityRequestLabelLengthRule(int MaxLength) : IMobilityRequestRule
{
    public List<string> Check(MobilityRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityRequestCodePrefixRule(string Prefix) : IMobilityRequestRule
{
    public List<string> Check(MobilityRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityRequestLocationLengthRule(int MaxLength) : IMobilityRequestRule
{
    public List<string> Check(MobilityRequest item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility request.</summary>
public sealed record MobilityRequestValidator(List<IMobilityRequestRule> Rules)
{
    public List<string> Validate(MobilityRequest item)
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

    public void RequireValid(MobilityRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityRequest> items)
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

/// <summary>Functions of mobility request rules.</summary>
public static class MobilityRequestRules
{
    public static MobilityRequestValidator DefaultMobilityRequestValidator()
    {
        return new MobilityRequestValidator(
            new List<IMobilityRequestRule>
            {
                new MobilityRequestRequiredLabelRule("mobility request needs an entry"),
                new MobilityRequestLabelLengthRule(120),
                new MobilityRequestCodePrefixRule("Q"),
                new MobilityRequestLocationLengthRule(64),
            });
    }
}
