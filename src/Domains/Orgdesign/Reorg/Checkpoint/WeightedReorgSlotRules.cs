namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>A rule a weighted reorg slot must pass. Check returns its problems, or none.</summary>
public interface IWeightedReorgSlotRule
{
    List<string> Check(WeightedReorgSlot item);
}

public sealed record WeightedReorgSlotRequiredCodeRule(string Message) : IWeightedReorgSlotRule
{
    public List<string> Check(WeightedReorgSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgSlotRequiredSummaryRule(string Message) : IWeightedReorgSlotRule
{
    public List<string> Check(WeightedReorgSlot item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgSlotCurrencyLengthRule(int MaxLength) : IWeightedReorgSlotRule
{
    public List<string> Check(WeightedReorgSlot item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted reorg slot.</summary>
public sealed record WeightedReorgSlotValidator(List<IWeightedReorgSlotRule> Rules)
{
    public List<string> Validate(WeightedReorgSlot item)
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

    public void RequireValid(WeightedReorgSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedReorgSlot> items)
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

/// <summary>Functions of weighted reorg slot rules.</summary>
public static class WeightedReorgSlotRules
{
    public static WeightedReorgSlotValidator DefaultWeightedReorgSlotValidator()
    {
        return new WeightedReorgSlotValidator(
            new List<IWeightedReorgSlotRule>
            {
                new WeightedReorgSlotRequiredCodeRule("weighted reorg slot needs a value"),
                new WeightedReorgSlotRequiredSummaryRule("weighted reorg slot needs a value"),
                new WeightedReorgSlotCurrencyLengthRule(3600),
            });
    }
}
