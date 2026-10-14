namespace Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>A rule a legacy license seat window must pass. Check returns its problems, or none.</summary>
public interface ILegacyLicenseSeatWindowRule
{
    List<string> Check(LegacyLicenseSeatWindow item);
}

public sealed record LegacyLicenseSeatWindowRequiredNameRule(string Message) : ILegacyLicenseSeatWindowRule
{
    public List<string> Check(LegacyLicenseSeatWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatWindowRequiredJobCodeRule(string Message) : ILegacyLicenseSeatWindowRule
{
    public List<string> Check(LegacyLicenseSeatWindow item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatWindowCodePrefixRule(string Prefix) : ILegacyLicenseSeatWindowRule
{
    public List<string> Check(LegacyLicenseSeatWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatWindowCodeLengthRule(int MaxLength) : ILegacyLicenseSeatWindowRule
{
    public List<string> Check(LegacyLicenseSeatWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatWindowTitleLengthRule(int MaxLength) : ILegacyLicenseSeatWindowRule
{
    public List<string> Check(LegacyLicenseSeatWindow item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy license seat window.</summary>
public sealed record LegacyLicenseSeatWindowValidator(List<ILegacyLicenseSeatWindowRule> Rules)
{
    public List<string> Validate(LegacyLicenseSeatWindow item)
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

    public void RequireValid(LegacyLicenseSeatWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLicenseSeatWindow> items)
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

/// <summary>Functions of legacy license seat window rules.</summary>
public static class LegacyLicenseSeatWindowRules
{
    public static LegacyLicenseSeatWindowValidator DefaultLegacyLicenseSeatWindowValidator()
    {
        return new LegacyLicenseSeatWindowValidator(
            new List<ILegacyLicenseSeatWindowRule>
            {
                new LegacyLicenseSeatWindowRequiredNameRule("legacy license seat window needs a value"),
                new LegacyLicenseSeatWindowRequiredJobCodeRule("legacy license seat window needs an entry"),
                new LegacyLicenseSeatWindowCodePrefixRule("Q"),
                new LegacyLicenseSeatWindowCodeLengthRule(64),
                new LegacyLicenseSeatWindowTitleLengthRule(64),
            });
    }
}
