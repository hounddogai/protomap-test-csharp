namespace Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>A rule a pay band exception must pass. Check returns its problems, or none.</summary>
public interface IPayBandExceptionRule
{
    List<string> Check(PayBandException item);
}

public sealed record PayBandExceptionCodePrefixRule(string Prefix) : IPayBandExceptionRule
{
    public List<string> Check(PayBandException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandExceptionRequiredSkillRule(string Message) : IPayBandExceptionRule
{
    public List<string> Check(PayBandException item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandExceptionVersionRangeRule(int Min, int Max) : IPayBandExceptionRule
{
    public List<string> Check(PayBandException item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandExceptionDurationMinutesRangeRule(int Min, int Max) : IPayBandExceptionRule
{
    public List<string> Check(PayBandException item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandExceptionSummaryLengthRule(int MaxLength) : IPayBandExceptionRule
{
    public List<string> Check(PayBandException item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band exception.</summary>
public sealed record PayBandExceptionValidator(List<IPayBandExceptionRule> Rules)
{
    public List<string> Validate(PayBandException item)
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

    public void RequireValid(PayBandException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandException> items)
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

/// <summary>Functions of pay band exception rules.</summary>
public static class PayBandExceptionRules
{
    public static PayBandExceptionValidator DefaultPayBandExceptionValidator()
    {
        return new PayBandExceptionValidator(
            new List<IPayBandExceptionRule>
            {
                new PayBandExceptionCodePrefixRule("REF"),
                new PayBandExceptionRequiredSkillRule("pay band exception needs a value"),
                new PayBandExceptionVersionRangeRule(1000, 3600),
                new PayBandExceptionDurationMinutesRangeRule(32, 80),
                new PayBandExceptionSummaryLengthRule(120),
            });
    }
}
