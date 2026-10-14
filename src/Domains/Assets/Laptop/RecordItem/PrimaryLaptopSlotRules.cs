namespace Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>A rule a primary laptop slot must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLaptopSlotRule
{
    List<string> Check(PrimaryLaptopSlot item);
}

public sealed record PrimaryLaptopSlotRequiredCodeRule(string Message) : IPrimaryLaptopSlotRule
{
    public List<string> Check(PrimaryLaptopSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLaptopSlotTitleLengthRule(int MaxLength) : IPrimaryLaptopSlotRule
{
    public List<string> Check(PrimaryLaptopSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLaptopSlotCodeLengthRule(int MaxLength) : IPrimaryLaptopSlotRule
{
    public List<string> Check(PrimaryLaptopSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLaptopSlotCodePrefixRule(string Prefix) : IPrimaryLaptopSlotRule
{
    public List<string> Check(PrimaryLaptopSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary laptop slot.</summary>
public sealed record PrimaryLaptopSlotValidator(List<IPrimaryLaptopSlotRule> Rules)
{
    public List<string> Validate(PrimaryLaptopSlot item)
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

    public void RequireValid(PrimaryLaptopSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLaptopSlot> items)
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

/// <summary>Functions of primary laptop slot rules.</summary>
public static class PrimaryLaptopSlotRules
{
    public static PrimaryLaptopSlotValidator DefaultPrimaryLaptopSlotValidator()
    {
        return new PrimaryLaptopSlotValidator(
            new List<IPrimaryLaptopSlotRule>
            {
                new PrimaryLaptopSlotRequiredCodeRule("primary laptop slot needs this field"),
                new PrimaryLaptopSlotTitleLengthRule(32),
                new PrimaryLaptopSlotCodeLengthRule(1000),
                new PrimaryLaptopSlotCodePrefixRule("REF"),
            });
    }
}
