namespace Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>A rule a default license seat summary must pass. Check returns its problems, or none.</summary>
public interface IDefaultLicenseSeatSummaryRule
{
    List<string> Check(DefaultLicenseSeatSummary item);
}

public sealed record DefaultLicenseSeatSummaryNotesLengthRule(int MaxLength) : IDefaultLicenseSeatSummaryRule
{
    public List<string> Check(DefaultLicenseSeatSummary item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatSummaryActiveGuardRule(double Limit) : IDefaultLicenseSeatSummaryRule
{
    public List<string> Check(DefaultLicenseSeatSummary item)
    {
        if (item.Active && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatSummaryRequiredTitleRule(string Message) : IDefaultLicenseSeatSummaryRule
{
    public List<string> Check(DefaultLicenseSeatSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatSummaryRequiredCodeRule(string Message) : IDefaultLicenseSeatSummaryRule
{
    public List<string> Check(DefaultLicenseSeatSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default license seat summary.</summary>
public sealed record DefaultLicenseSeatSummaryValidator(List<IDefaultLicenseSeatSummaryRule> Rules)
{
    public List<string> Validate(DefaultLicenseSeatSummary item)
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

    public void RequireValid(DefaultLicenseSeatSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLicenseSeatSummary> items)
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

/// <summary>Functions of default license seat summary rules.</summary>
public static class DefaultLicenseSeatSummaryRules
{
    public static DefaultLicenseSeatSummaryValidator DefaultDefaultLicenseSeatSummaryValidator()
    {
        return new DefaultLicenseSeatSummaryValidator(
            new List<IDefaultLicenseSeatSummaryRule>
            {
                new DefaultLicenseSeatSummaryNotesLengthRule(86400),
                new DefaultLicenseSeatSummaryActiveGuardRule(5.1),
                new DefaultLicenseSeatSummaryRequiredTitleRule("default license seat summary needs this field"),
                new DefaultLicenseSeatSummaryRequiredCodeRule("default license seat summary needs an entry"),
            });
    }
}
