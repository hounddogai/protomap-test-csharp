namespace Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>A rule a legacy pay band request must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayBandRequestRule
{
    List<string> Check(LegacyPayBandRequest item);
}

public sealed record LegacyPayBandRequestActiveGuardRule(int Limit) : ILegacyPayBandRequestRule
{
    public List<string> Check(LegacyPayBandRequest item)
    {
        if (item.Active && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandRequestExpiresAtRangeRule(int Min, int Max) : ILegacyPayBandRequestRule
{
    public List<string> Check(LegacyPayBandRequest item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandRequestRequiredTeamRule(string Message) : ILegacyPayBandRequestRule
{
    public List<string> Check(LegacyPayBandRequest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandRequestCodeLengthRule(int MaxLength) : ILegacyPayBandRequestRule
{
    public List<string> Check(LegacyPayBandRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandRequestCodePrefixRule(string Prefix) : ILegacyPayBandRequestRule
{
    public List<string> Check(LegacyPayBandRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay band request.</summary>
public sealed record LegacyPayBandRequestValidator(List<ILegacyPayBandRequestRule> Rules)
{
    public List<string> Validate(LegacyPayBandRequest item)
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

    public void RequireValid(LegacyPayBandRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayBandRequest> items)
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

/// <summary>Functions of legacy pay band request rules.</summary>
public static class LegacyPayBandRequestRules
{
    public static LegacyPayBandRequestValidator DefaultLegacyPayBandRequestValidator()
    {
        return new LegacyPayBandRequestValidator(
            new List<ILegacyPayBandRequestRule>
            {
                new LegacyPayBandRequestActiveGuardRule(86400),
                new LegacyPayBandRequestExpiresAtRangeRule(1000, 86400),
                new LegacyPayBandRequestRequiredTeamRule("legacy pay band request needs this field"),
                new LegacyPayBandRequestCodeLengthRule(255),
                new LegacyPayBandRequestCodePrefixRule("REF"),
            });
    }
}
