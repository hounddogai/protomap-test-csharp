namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>A rule a license seat window must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatWindowRule
{
    List<string> Check(LicenseSeatWindow item);
}

public sealed record LicenseSeatWindowRequiredDepartmentRule(string Message) : ILicenseSeatWindowRule
{
    public List<string> Check(LicenseSeatWindow item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatWindowRequiredTitleRule(string Message) : ILicenseSeatWindowRule
{
    public List<string> Check(LicenseSeatWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatWindowCodePrefixRule(string Prefix) : ILicenseSeatWindowRule
{
    public List<string> Check(LicenseSeatWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat window.</summary>
public sealed record LicenseSeatWindowValidator(List<ILicenseSeatWindowRule> Rules)
{
    public List<string> Validate(LicenseSeatWindow item)
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

    public void RequireValid(LicenseSeatWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatWindow> items)
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

/// <summary>Functions of license seat window rules.</summary>
public static class LicenseSeatWindowRules
{
    public static LicenseSeatWindowValidator DefaultLicenseSeatWindowValidator()
    {
        return new LicenseSeatWindowValidator(
            new List<ILicenseSeatWindowRule>
            {
                new LicenseSeatWindowRequiredDepartmentRule("license seat window needs this field"),
                new LicenseSeatWindowRequiredTitleRule("license seat window needs an entry"),
                new LicenseSeatWindowCodePrefixRule("REF"),
            });
    }
}
