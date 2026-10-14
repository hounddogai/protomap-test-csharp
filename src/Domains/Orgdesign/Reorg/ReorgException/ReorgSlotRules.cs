namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>A rule a reorg slot must pass. Check returns its problems, or none.</summary>
public interface IReorgSlotRule
{
    List<string> Check(ReorgSlot item);
}

public sealed record ReorgSlotCodeLengthRule(int MaxLength) : IReorgSlotRule
{
    public List<string> Check(ReorgSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgSlotThresholdRangeRule(double Min, double Max) : IReorgSlotRule
{
    public List<string> Check(ReorgSlot item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgSlotRequiredGradeRule(string Message) : IReorgSlotRule
{
    public List<string> Check(ReorgSlot item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg slot.</summary>
public sealed record ReorgSlotValidator(List<IReorgSlotRule> Rules)
{
    public List<string> Validate(ReorgSlot item)
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

    public void RequireValid(ReorgSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgSlot> items)
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

/// <summary>Functions of reorg slot rules.</summary>
public static class ReorgSlotRules
{
    public static ReorgSlotValidator DefaultReorgSlotValidator()
    {
        return new ReorgSlotValidator(
            new List<IReorgSlotRule>
            {
                new ReorgSlotCodeLengthRule(16),
                new ReorgSlotThresholdRangeRule(2.7, 1839.2),
                new ReorgSlotRequiredGradeRule("reorg slot needs an entry"),
            });
    }
}
