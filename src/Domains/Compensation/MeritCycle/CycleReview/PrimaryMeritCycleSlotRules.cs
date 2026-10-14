namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary merit cycle slot must pass. Check returns its problems, or none.</summary>
public interface IPrimaryMeritCycleSlotRule
{
    List<string> Check(PrimaryMeritCycleSlot item);
}

public sealed record PrimaryMeritCycleSlotRequiredChecksumRule(string Message) : IPrimaryMeritCycleSlotRule
{
    public List<string> Check(PrimaryMeritCycleSlot item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleSlotTagsLimitRule(int MaxCount, List<string> Banned) : IPrimaryMeritCycleSlotRule
{
    public List<string> Check(PrimaryMeritCycleSlot item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record PrimaryMeritCycleSlotNameLengthRule(int MaxLength) : IPrimaryMeritCycleSlotRule
{
    public List<string> Check(PrimaryMeritCycleSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleSlotAllowedCodeRule(List<string> Allowed) : IPrimaryMeritCycleSlotRule
{
    public List<string> Check(PrimaryMeritCycleSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryMeritCycleSlotCodePrefixRule(string Prefix) : IPrimaryMeritCycleSlotRule
{
    public List<string> Check(PrimaryMeritCycleSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary merit cycle slot.</summary>
public sealed record PrimaryMeritCycleSlotValidator(List<IPrimaryMeritCycleSlotRule> Rules)
{
    public List<string> Validate(PrimaryMeritCycleSlot item)
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

    public void RequireValid(PrimaryMeritCycleSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryMeritCycleSlot> items)
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

/// <summary>Functions of primary merit cycle slot rules.</summary>
public static class PrimaryMeritCycleSlotRules
{
    public static PrimaryMeritCycleSlotValidator DefaultPrimaryMeritCycleSlotValidator()
    {
        return new PrimaryMeritCycleSlotValidator(
            new List<IPrimaryMeritCycleSlotRule>
            {
                new PrimaryMeritCycleSlotRequiredChecksumRule("primary merit cycle slot needs a value"),
                new PrimaryMeritCycleSlotTagsLimitRule(16, new List<string>()),
                new PrimaryMeritCycleSlotNameLengthRule(1000),
                new PrimaryMeritCycleSlotAllowedCodeRule(new List<string> { "global", "rolling" }),
                new PrimaryMeritCycleSlotCodePrefixRule("OPS"),
            });
    }
}
