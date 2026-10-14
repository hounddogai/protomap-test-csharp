namespace Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>A rule a default license seat profile must pass. Check returns its problems, or none.</summary>
public interface IDefaultLicenseSeatProfileRule
{
    List<string> Check(DefaultLicenseSeatProfile item);
}

public sealed record DefaultLicenseSeatProfileRequiredCodeRule(string Message) : IDefaultLicenseSeatProfileRule
{
    public List<string> Check(DefaultLicenseSeatProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatProfileRequiredSourceRule(string Message) : IDefaultLicenseSeatProfileRule
{
    public List<string> Check(DefaultLicenseSeatProfile item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatProfileSourceLengthRule(int MaxLength) : IDefaultLicenseSeatProfileRule
{
    public List<string> Check(DefaultLicenseSeatProfile item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default license seat profile.</summary>
public sealed record DefaultLicenseSeatProfileValidator(List<IDefaultLicenseSeatProfileRule> Rules)
{
    public List<string> Validate(DefaultLicenseSeatProfile item)
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

    public void RequireValid(DefaultLicenseSeatProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLicenseSeatProfile> items)
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

/// <summary>Functions of default license seat profile rules.</summary>
public static class DefaultLicenseSeatProfileRules
{
    public static DefaultLicenseSeatProfileValidator DefaultDefaultLicenseSeatProfileValidator()
    {
        return new DefaultLicenseSeatProfileValidator(
            new List<IDefaultLicenseSeatProfileRule>
            {
                new DefaultLicenseSeatProfileRequiredCodeRule("default license seat profile needs an entry"),
                new DefaultLicenseSeatProfileRequiredSourceRule("default license seat profile needs an entry"),
                new DefaultLicenseSeatProfileSourceLengthRule(80),
            });
    }
}
