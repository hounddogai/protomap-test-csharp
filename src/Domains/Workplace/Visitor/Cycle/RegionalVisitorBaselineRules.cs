namespace Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>A rule a regional visitor baseline must pass. Check returns its problems, or none.</summary>
public interface IRegionalVisitorBaselineRule
{
    List<string> Check(RegionalVisitorBaseline item);
}

public sealed record RegionalVisitorBaselinePhoneLengthRule(int MaxLength) : IRegionalVisitorBaselineRule
{
    public List<string> Check(RegionalVisitorBaseline item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalVisitorBaselineTitleLengthRule(int MaxLength) : IRegionalVisitorBaselineRule
{
    public List<string> Check(RegionalVisitorBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalVisitorBaselineRequiredManagerIdRule(string Message) : IRegionalVisitorBaselineRule
{
    public List<string> Check(RegionalVisitorBaseline item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalVisitorBaselineCodePrefixRule(string Prefix) : IRegionalVisitorBaselineRule
{
    public List<string> Check(RegionalVisitorBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional visitor baseline.</summary>
public sealed record RegionalVisitorBaselineValidator(List<IRegionalVisitorBaselineRule> Rules)
{
    public List<string> Validate(RegionalVisitorBaseline item)
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

    public void RequireValid(RegionalVisitorBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalVisitorBaseline> items)
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

/// <summary>Functions of regional visitor baseline rules.</summary>
public static class RegionalVisitorBaselineRules
{
    public static RegionalVisitorBaselineValidator DefaultRegionalVisitorBaselineValidator()
    {
        return new RegionalVisitorBaselineValidator(
            new List<IRegionalVisitorBaselineRule>
            {
                new RegionalVisitorBaselinePhoneLengthRule(255),
                new RegionalVisitorBaselineTitleLengthRule(86400),
                new RegionalVisitorBaselineRequiredManagerIdRule("regional visitor baseline needs an entry"),
                new RegionalVisitorBaselineCodePrefixRule("X"),
            });
    }
}
