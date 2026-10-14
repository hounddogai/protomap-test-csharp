namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop snapshot must pass. Check returns its problems, or none.</summary>
public interface ILaptopSnapshotRule
{
    List<string> Check(LaptopSnapshot item);
}

public sealed record LaptopSnapshotVisibleGuardRule(int Limit) : ILaptopSnapshotRule
{
    public List<string> Check(LaptopSnapshot item)
    {
        if (item.Visible && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LaptopSnapshotAllowedOwnerIdRule(List<string> Allowed) : ILaptopSnapshotRule
{
    public List<string> Check(LaptopSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LaptopSnapshotChecksumLengthRule(int MaxLength) : ILaptopSnapshotRule
{
    public List<string> Check(LaptopSnapshot item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopSnapshotRequiredChecksumRule(string Message) : ILaptopSnapshotRule
{
    public List<string> Check(LaptopSnapshot item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopSnapshotRequiredOwnerIdRule(string Message) : ILaptopSnapshotRule
{
    public List<string> Check(LaptopSnapshot item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopSnapshotRequiredCodeRule(string Message) : ILaptopSnapshotRule
{
    public List<string> Check(LaptopSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop snapshot.</summary>
public sealed record LaptopSnapshotValidator(List<ILaptopSnapshotRule> Rules)
{
    public List<string> Validate(LaptopSnapshot item)
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

    public void RequireValid(LaptopSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopSnapshot> items)
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

/// <summary>Functions of laptop snapshot rules.</summary>
public static class LaptopSnapshotRules
{
    public static LaptopSnapshotValidator DefaultLaptopSnapshotValidator()
    {
        return new LaptopSnapshotValidator(
            new List<ILaptopSnapshotRule>
            {
                new LaptopSnapshotVisibleGuardRule(32),
                new LaptopSnapshotAllowedOwnerIdRule(new List<string>()),
                new LaptopSnapshotChecksumLengthRule(32),
                new LaptopSnapshotRequiredChecksumRule("laptop snapshot needs a value"),
                new LaptopSnapshotRequiredOwnerIdRule("laptop snapshot needs an entry"),
                new LaptopSnapshotRequiredCodeRule("laptop snapshot needs this field"),
            });
    }
}
