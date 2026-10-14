namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation schedule must pass. Check returns its problems, or none.</summary>
public interface IAttestationScheduleRule
{
    List<string> Check(AttestationSchedule item);
}

public sealed record AttestationScheduleCurrencyLengthRule(int MaxLength) : IAttestationScheduleRule
{
    public List<string> Check(AttestationSchedule item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationScheduleRequiredUnitRule(string Message) : IAttestationScheduleRule
{
    public List<string> Check(AttestationSchedule item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationScheduleAllowedCodeRule(List<string> Allowed) : IAttestationScheduleRule
{
    public List<string> Check(AttestationSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AttestationScheduleUnitLengthRule(int MaxLength) : IAttestationScheduleRule
{
    public List<string> Check(AttestationSchedule item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationScheduleNameLengthRule(int MaxLength) : IAttestationScheduleRule
{
    public List<string> Check(AttestationSchedule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationScheduleCodeLengthRule(int MaxLength) : IAttestationScheduleRule
{
    public List<string> Check(AttestationSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation schedule.</summary>
public sealed record AttestationScheduleValidator(List<IAttestationScheduleRule> Rules)
{
    public List<string> Validate(AttestationSchedule item)
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

    public void RequireValid(AttestationSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationSchedule> items)
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

/// <summary>Functions of attestation schedule rules.</summary>
public static class AttestationScheduleRules
{
    public static AttestationScheduleValidator DefaultAttestationScheduleValidator()
    {
        return new AttestationScheduleValidator(
            new List<IAttestationScheduleRule>
            {
                new AttestationScheduleCurrencyLengthRule(16),
                new AttestationScheduleRequiredUnitRule("attestation schedule needs an entry"),
                new AttestationScheduleAllowedCodeRule(new List<string> { "global", "lenient" }),
                new AttestationScheduleUnitLengthRule(3600),
                new AttestationScheduleNameLengthRule(1000),
                new AttestationScheduleCodeLengthRule(86400),
            });
    }
}
