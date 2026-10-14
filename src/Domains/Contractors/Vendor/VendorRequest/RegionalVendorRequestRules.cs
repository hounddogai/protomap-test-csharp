namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

/// <summary>A rule a regional vendor request must pass. Check returns its problems, or none.</summary>
public interface IRegionalVendorRequestRule
{
    List<string> Check(RegionalVendorRequest item);
}

public sealed record RegionalVendorRequestScoreRangeRule(double Min, double Max) : IRegionalVendorRequestRule
{
    public List<string> Check(RegionalVendorRequest item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalVendorRequestRequiredCodeRule(string Message) : IRegionalVendorRequestRule
{
    public List<string> Check(RegionalVendorRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalVendorRequestTitleLengthRule(int MaxLength) : IRegionalVendorRequestRule
{
    public List<string> Check(RegionalVendorRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalVendorRequestLocationLengthRule(int MaxLength) : IRegionalVendorRequestRule
{
    public List<string> Check(RegionalVendorRequest item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional vendor request.</summary>
public sealed record RegionalVendorRequestValidator(List<IRegionalVendorRequestRule> Rules)
{
    public List<string> Validate(RegionalVendorRequest item)
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

    public void RequireValid(RegionalVendorRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalVendorRequest> items)
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

/// <summary>Functions of regional vendor request rules.</summary>
public static class RegionalVendorRequestRules
{
    public static RegionalVendorRequestValidator DefaultRegionalVendorRequestValidator()
    {
        return new RegionalVendorRequestValidator(
            new List<IRegionalVendorRequestRule>
            {
                new RegionalVendorRequestScoreRangeRule(9.3, 4106.9),
                new RegionalVendorRequestRequiredCodeRule("regional vendor request needs this field"),
                new RegionalVendorRequestTitleLengthRule(64),
                new RegionalVendorRequestLocationLengthRule(255),
            });
    }
}
