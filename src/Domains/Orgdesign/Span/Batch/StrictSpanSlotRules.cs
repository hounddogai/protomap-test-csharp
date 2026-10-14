namespace Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>A rule a strict span slot must pass. Check returns its problems, or none.</summary>
public interface IStrictSpanSlotRule
{
    List<string> Check(StrictSpanSlot item);
}

public sealed record StrictSpanSlotLabelLengthRule(int MaxLength) : IStrictSpanSlotRule
{
    public List<string> Check(StrictSpanSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSpanSlotRequiredUnitRule(string Message) : IStrictSpanSlotRule
{
    public List<string> Check(StrictSpanSlot item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSpanSlotAmountRangeRule(double Min, double Max) : IStrictSpanSlotRule
{
    public List<string> Check(StrictSpanSlot item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSpanSlotUnitLengthRule(int MaxLength) : IStrictSpanSlotRule
{
    public List<string> Check(StrictSpanSlot item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict span slot.</summary>
public sealed record StrictSpanSlotValidator(List<IStrictSpanSlotRule> Rules)
{
    public List<string> Validate(StrictSpanSlot item)
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

    public void RequireValid(StrictSpanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictSpanSlot> items)
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

/// <summary>Functions of strict span slot rules.</summary>
public static class StrictSpanSlotRules
{
    public static StrictSpanSlotValidator DefaultStrictSpanSlotValidator()
    {
        return new StrictSpanSlotValidator(
            new List<IStrictSpanSlotRule>
            {
                new StrictSpanSlotLabelLengthRule(80),
                new StrictSpanSlotRequiredUnitRule("strict span slot needs this field"),
                new StrictSpanSlotAmountRangeRule(7.6, 256.1),
                new StrictSpanSlotUnitLengthRule(16),
            });
    }
}
