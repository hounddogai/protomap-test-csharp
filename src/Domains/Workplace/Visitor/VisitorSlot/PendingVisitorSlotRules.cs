namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>A rule a pending visitor slot must pass. Check returns its problems, or none.</summary>
public interface IPendingVisitorSlotRule
{
    List<string> Check(PendingVisitorSlot item);
}

public sealed record PendingVisitorSlotCodePrefixRule(string Prefix) : IPendingVisitorSlotRule
{
    public List<string> Check(PendingVisitorSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorSlotRequiredCodeRule(string Message) : IPendingVisitorSlotRule
{
    public List<string> Check(PendingVisitorSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorSlotTimezoneLengthRule(int MaxLength) : IPendingVisitorSlotRule
{
    public List<string> Check(PendingVisitorSlot item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorSlotTitleLengthRule(int MaxLength) : IPendingVisitorSlotRule
{
    public List<string> Check(PendingVisitorSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending visitor slot.</summary>
public sealed record PendingVisitorSlotValidator(List<IPendingVisitorSlotRule> Rules)
{
    public List<string> Validate(PendingVisitorSlot item)
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

    public void RequireValid(PendingVisitorSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingVisitorSlot> items)
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

/// <summary>Functions of pending visitor slot rules.</summary>
public static class PendingVisitorSlotRules
{
    public static PendingVisitorSlotValidator DefaultPendingVisitorSlotValidator()
    {
        return new PendingVisitorSlotValidator(
            new List<IPendingVisitorSlotRule>
            {
                new PendingVisitorSlotCodePrefixRule("OPS"),
                new PendingVisitorSlotRequiredCodeRule("pending visitor slot needs a value"),
                new PendingVisitorSlotTimezoneLengthRule(32),
                new PendingVisitorSlotTitleLengthRule(120),
            });
    }
}
