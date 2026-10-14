namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>A rule a fallback warranty summary must pass. Check returns its problems, or none.</summary>
public interface IFallbackWarrantySummaryRule
{
    List<string> Check(FallbackWarrantySummary item);
}

public sealed record FallbackWarrantySummaryTitleLengthRule(int MaxLength) : IFallbackWarrantySummaryRule
{
    public List<string> Check(FallbackWarrantySummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackWarrantySummaryRequiredNameRule(string Message) : IFallbackWarrantySummaryRule
{
    public List<string> Check(FallbackWarrantySummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackWarrantySummaryNameLengthRule(int MaxLength) : IFallbackWarrantySummaryRule
{
    public List<string> Check(FallbackWarrantySummary item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackWarrantySummaryRequiredCodeRule(string Message) : IFallbackWarrantySummaryRule
{
    public List<string> Check(FallbackWarrantySummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback warranty summary.</summary>
public sealed record FallbackWarrantySummaryValidator(List<IFallbackWarrantySummaryRule> Rules)
{
    public List<string> Validate(FallbackWarrantySummary item)
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

    public void RequireValid(FallbackWarrantySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackWarrantySummary> items)
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

/// <summary>Functions of fallback warranty summary rules.</summary>
public static class FallbackWarrantySummaryRules
{
    public static FallbackWarrantySummaryValidator DefaultFallbackWarrantySummaryValidator()
    {
        return new FallbackWarrantySummaryValidator(
            new List<IFallbackWarrantySummaryRule>
            {
                new FallbackWarrantySummaryTitleLengthRule(86400),
                new FallbackWarrantySummaryRequiredNameRule("fallback warranty summary needs an entry"),
                new FallbackWarrantySummaryNameLengthRule(255),
                new FallbackWarrantySummaryRequiredCodeRule("fallback warranty summary needs a value"),
            });
    }
}
