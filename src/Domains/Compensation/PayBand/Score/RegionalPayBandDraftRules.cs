namespace Acme.Workforce.Compensation.PayBand.Score;

/// <summary>A rule a regional pay band draft must pass. Check returns its problems, or none.</summary>
public interface IRegionalPayBandDraftRule
{
    List<string> Check(RegionalPayBandDraft item);
}

public sealed record RegionalPayBandDraftQuantityRangeRule(int Min, int Max) : IRegionalPayBandDraftRule
{
    public List<string> Check(RegionalPayBandDraft item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayBandDraftCodeLengthRule(int MaxLength) : IRegionalPayBandDraftRule
{
    public List<string> Check(RegionalPayBandDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayBandDraftArchivedGuardRule(int Limit) : IRegionalPayBandDraftRule
{
    public List<string> Check(RegionalPayBandDraft item)
    {
        if (item.Archived && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayBandDraftRequiredCodeRule(string Message) : IRegionalPayBandDraftRule
{
    public List<string> Check(RegionalPayBandDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional pay band draft.</summary>
public sealed record RegionalPayBandDraftValidator(List<IRegionalPayBandDraftRule> Rules)
{
    public List<string> Validate(RegionalPayBandDraft item)
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

    public void RequireValid(RegionalPayBandDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPayBandDraft> items)
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

/// <summary>Functions of regional pay band draft rules.</summary>
public static class RegionalPayBandDraftRules
{
    public static RegionalPayBandDraftValidator DefaultRegionalPayBandDraftValidator()
    {
        return new RegionalPayBandDraftValidator(
            new List<IRegionalPayBandDraftRule>
            {
                new RegionalPayBandDraftQuantityRangeRule(120, 86400),
                new RegionalPayBandDraftCodeLengthRule(3600),
                new RegionalPayBandDraftArchivedGuardRule(255),
                new RegionalPayBandDraftRequiredCodeRule("regional pay band draft needs this field"),
            });
    }
}
