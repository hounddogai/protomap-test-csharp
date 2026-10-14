namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>A rule a regional attestation schedule must pass. Check returns its problems, or none.</summary>
public interface IRegionalAttestationScheduleRule
{
    List<string> Check(RegionalAttestationSchedule item);
}

public sealed record RegionalAttestationScheduleActiveGuardRule(double Limit) : IRegionalAttestationScheduleRule
{
    public List<string> Check(RegionalAttestationSchedule item)
    {
        if (item.Active && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAttestationScheduleMaximumRangeRule(double Min, double Max) : IRegionalAttestationScheduleRule
{
    public List<string> Check(RegionalAttestationSchedule item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAttestationScheduleCodeLengthRule(int MaxLength) : IRegionalAttestationScheduleRule
{
    public List<string> Check(RegionalAttestationSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAttestationScheduleRequiredNameRule(string Message) : IRegionalAttestationScheduleRule
{
    public List<string> Check(RegionalAttestationSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional attestation schedule.</summary>
public sealed record RegionalAttestationScheduleValidator(List<IRegionalAttestationScheduleRule> Rules)
{
    public List<string> Validate(RegionalAttestationSchedule item)
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

    public void RequireValid(RegionalAttestationSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAttestationSchedule> items)
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

/// <summary>Functions of regional attestation schedule rules.</summary>
public static class RegionalAttestationScheduleRules
{
    public static RegionalAttestationScheduleValidator DefaultRegionalAttestationScheduleValidator()
    {
        return new RegionalAttestationScheduleValidator(
            new List<IRegionalAttestationScheduleRule>
            {
                new RegionalAttestationScheduleActiveGuardRule(8.0),
                new RegionalAttestationScheduleMaximumRangeRule(7.7, 239.3),
                new RegionalAttestationScheduleCodeLengthRule(16),
                new RegionalAttestationScheduleRequiredNameRule("regional attestation schedule needs this field"),
            });
    }
}
