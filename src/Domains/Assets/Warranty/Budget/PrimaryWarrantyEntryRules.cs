namespace Acme.Workforce.Assets.Warranty.Budget;

/// <summary>A rule a primary warranty entry must pass. Check returns its problems, or none.</summary>
public interface IPrimaryWarrantyEntryRule
{
    List<string> Check(PrimaryWarrantyEntry item);
}

public sealed record PrimaryWarrantyEntryRevisionRangeRule(int Min, int Max) : IPrimaryWarrantyEntryRule
{
    public List<string> Check(PrimaryWarrantyEntry item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryWarrantyEntryCodeLengthRule(int MaxLength) : IPrimaryWarrantyEntryRule
{
    public List<string> Check(PrimaryWarrantyEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryWarrantyEntryTitleLengthRule(int MaxLength) : IPrimaryWarrantyEntryRule
{
    public List<string> Check(PrimaryWarrantyEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary warranty entry.</summary>
public sealed record PrimaryWarrantyEntryValidator(List<IPrimaryWarrantyEntryRule> Rules)
{
    public List<string> Validate(PrimaryWarrantyEntry item)
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

    public void RequireValid(PrimaryWarrantyEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryWarrantyEntry> items)
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

/// <summary>Functions of primary warranty entry rules.</summary>
public static class PrimaryWarrantyEntryRules
{
    public static PrimaryWarrantyEntryValidator DefaultPrimaryWarrantyEntryValidator()
    {
        return new PrimaryWarrantyEntryValidator(
            new List<IPrimaryWarrantyEntryRule>
            {
                new PrimaryWarrantyEntryRevisionRangeRule(32, 32),
                new PrimaryWarrantyEntryCodeLengthRule(80),
                new PrimaryWarrantyEntryTitleLengthRule(64),
            });
    }
}
