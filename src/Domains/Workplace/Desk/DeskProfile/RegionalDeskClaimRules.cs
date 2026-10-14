namespace Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>A rule a regional desk claim must pass. Check returns its problems, or none.</summary>
public interface IRegionalDeskClaimRule
{
    List<string> Check(RegionalDeskClaim item);
}

public sealed record RegionalDeskClaimRequiredNameRule(string Message) : IRegionalDeskClaimRule
{
    public List<string> Check(RegionalDeskClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskClaimCodeLengthRule(int MaxLength) : IRegionalDeskClaimRule
{
    public List<string> Check(RegionalDeskClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskClaimBatchSizeRangeRule(int Min, int Max) : IRegionalDeskClaimRule
{
    public List<string> Check(RegionalDeskClaim item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskClaimRequiredNotesRule(string Message) : IRegionalDeskClaimRule
{
    public List<string> Check(RegionalDeskClaim item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional desk claim.</summary>
public sealed record RegionalDeskClaimValidator(List<IRegionalDeskClaimRule> Rules)
{
    public List<string> Validate(RegionalDeskClaim item)
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

    public void RequireValid(RegionalDeskClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalDeskClaim> items)
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

/// <summary>Functions of regional desk claim rules.</summary>
public static class RegionalDeskClaimRules
{
    public static RegionalDeskClaimValidator DefaultRegionalDeskClaimValidator()
    {
        return new RegionalDeskClaimValidator(
            new List<IRegionalDeskClaimRule>
            {
                new RegionalDeskClaimRequiredNameRule("regional desk claim needs an entry"),
                new RegionalDeskClaimCodeLengthRule(255),
                new RegionalDeskClaimBatchSizeRangeRule(32, 120),
                new RegionalDeskClaimRequiredNotesRule("regional desk claim needs a value"),
            });
    }
}
