namespace Acme.Workforce.Compliance.Attestation.Baseline;

/// <summary>A rule a capped attestation window must pass. Check returns its problems, or none.</summary>
public interface ICappedAttestationWindowRule
{
    List<string> Check(CappedAttestationWindow item);
}

public sealed record CappedAttestationWindowRequiredCodeRule(string Message) : ICappedAttestationWindowRule
{
    public List<string> Check(CappedAttestationWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAttestationWindowRateRangeRule(double Min, double Max) : ICappedAttestationWindowRule
{
    public List<string> Check(CappedAttestationWindow item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAttestationWindowRequiredManagerIdRule(string Message) : ICappedAttestationWindowRule
{
    public List<string> Check(CappedAttestationWindow item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped attestation window.</summary>
public sealed record CappedAttestationWindowValidator(List<ICappedAttestationWindowRule> Rules)
{
    public List<string> Validate(CappedAttestationWindow item)
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

    public void RequireValid(CappedAttestationWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAttestationWindow> items)
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

/// <summary>Functions of capped attestation window rules.</summary>
public static class CappedAttestationWindowRules
{
    public static CappedAttestationWindowValidator DefaultCappedAttestationWindowValidator()
    {
        return new CappedAttestationWindowValidator(
            new List<ICappedAttestationWindowRule>
            {
                new CappedAttestationWindowRequiredCodeRule("capped attestation window needs this field"),
                new CappedAttestationWindowRateRangeRule(9.4, 1312.7),
                new CappedAttestationWindowRequiredManagerIdRule("capped attestation window needs this field"),
            });
    }
}
