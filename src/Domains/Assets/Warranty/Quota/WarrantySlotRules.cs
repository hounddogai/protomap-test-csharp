namespace Acme.Workforce.Assets.Warranty.Quota;

/// <summary>A rule a warranty slot must pass. Check returns its problems, or none.</summary>
public interface IWarrantySlotRule
{
    List<string> Check(WarrantySlot item);
}

public sealed record WarrantySlotRequiredLocationRule(string Message) : IWarrantySlotRule
{
    public List<string> Check(WarrantySlot item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantySlotArchivedGuardRule(int Limit) : IWarrantySlotRule
{
    public List<string> Check(WarrantySlot item)
    {
        if (item.Archived && item.DurationMinutes > Limit)
        {
            return new List<string> { $"duration minutes {item.DurationMinutes} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record WarrantySlotRequiredCodeRule(string Message) : IWarrantySlotRule
{
    public List<string> Check(WarrantySlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantySlotLabelLengthRule(int MaxLength) : IWarrantySlotRule
{
    public List<string> Check(WarrantySlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty slot.</summary>
public sealed record WarrantySlotValidator(List<IWarrantySlotRule> Rules)
{
    public List<string> Validate(WarrantySlot item)
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

    public void RequireValid(WarrantySlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantySlot> items)
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

/// <summary>Functions of warranty slot rules.</summary>
public static class WarrantySlotRules
{
    public static WarrantySlotValidator DefaultWarrantySlotValidator()
    {
        return new WarrantySlotValidator(
            new List<IWarrantySlotRule>
            {
                new WarrantySlotRequiredLocationRule("warranty slot needs this field"),
                new WarrantySlotArchivedGuardRule(32),
                new WarrantySlotRequiredCodeRule("warranty slot needs this field"),
                new WarrantySlotLabelLengthRule(64),
            });
    }
}
