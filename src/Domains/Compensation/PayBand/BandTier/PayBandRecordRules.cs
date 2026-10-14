namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band record must pass. Check returns its problems, or none.</summary>
public interface IPayBandRecordRule
{
    List<string> Check(PayBandRecord item);
}

public sealed record PayBandRecordRequiredNameRule(string Message) : IPayBandRecordRule
{
    public List<string> Check(PayBandRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandRecordNameLengthRule(int MaxLength) : IPayBandRecordRule
{
    public List<string> Check(PayBandRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRecordSourceLengthRule(int MaxLength) : IPayBandRecordRule
{
    public List<string> Check(PayBandRecord item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRecordAllowedTeamRule(List<string> Allowed) : IPayBandRecordRule
{
    public List<string> Check(PayBandRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRecordReviewerLengthRule(int MaxLength) : IPayBandRecordRule
{
    public List<string> Check(PayBandRecord item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band record.</summary>
public sealed record PayBandRecordValidator(List<IPayBandRecordRule> Rules)
{
    public List<string> Validate(PayBandRecord item)
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

    public void RequireValid(PayBandRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandRecord> items)
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

/// <summary>Functions of pay band record rules.</summary>
public static class PayBandRecordRules
{
    public static PayBandRecordValidator DefaultPayBandRecordValidator()
    {
        return new PayBandRecordValidator(
            new List<IPayBandRecordRule>
            {
                new PayBandRecordRequiredNameRule("pay band record needs an entry"),
                new PayBandRecordNameLengthRule(255),
                new PayBandRecordSourceLengthRule(255),
                new PayBandRecordAllowedTeamRule(new List<string>()),
                new PayBandRecordReviewerLengthRule(255),
            });
    }
}
