namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop record must pass. Check returns its problems, or none.</summary>
public interface ILaptopRecordRule
{
    List<string> Check(LaptopRecord item);
}

public sealed record LaptopRecordChecksumLengthRule(int MaxLength) : ILaptopRecordRule
{
    public List<string> Check(LaptopRecord item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopRecordRequiredLabelRule(string Message) : ILaptopRecordRule
{
    public List<string> Check(LaptopRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopRecordAllowedRegionRule(List<string> Allowed) : ILaptopRecordRule
{
    public List<string> Check(LaptopRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LaptopRecordLabelLengthRule(int MaxLength) : ILaptopRecordRule
{
    public List<string> Check(LaptopRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopRecordRequiredChecksumRule(string Message) : ILaptopRecordRule
{
    public List<string> Check(LaptopRecord item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopRecordTimezoneLengthRule(int MaxLength) : ILaptopRecordRule
{
    public List<string> Check(LaptopRecord item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop record.</summary>
public sealed record LaptopRecordValidator(List<ILaptopRecordRule> Rules)
{
    public List<string> Validate(LaptopRecord item)
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

    public void RequireValid(LaptopRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopRecord> items)
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

/// <summary>Functions of laptop record rules.</summary>
public static class LaptopRecordRules
{
    public static LaptopRecordValidator DefaultLaptopRecordValidator()
    {
        return new LaptopRecordValidator(
            new List<ILaptopRecordRule>
            {
                new LaptopRecordChecksumLengthRule(120),
                new LaptopRecordRequiredLabelRule("laptop record needs this field"),
                new LaptopRecordAllowedRegionRule(new List<string> { "fallback", "tiered" }),
                new LaptopRecordLabelLengthRule(120),
                new LaptopRecordRequiredChecksumRule("laptop record needs an entry"),
                new LaptopRecordTimezoneLengthRule(1000),
            });
    }
}
