namespace Acme.Workforce.Orgdesign.Reorg.Profile;

/// <summary>A rule a reorg profile must pass. Check returns its problems, or none.</summary>
public interface IReorgProfileRule
{
    List<string> Check(ReorgProfile item);
}

public sealed record ReorgProfileSequenceRangeRule(int Min, int Max) : IReorgProfileRule
{
    public List<string> Check(ReorgProfile item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgProfileRequiredLocationRule(string Message) : IReorgProfileRule
{
    public List<string> Check(ReorgProfile item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgProfileLabelLengthRule(int MaxLength) : IReorgProfileRule
{
    public List<string> Check(ReorgProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg profile.</summary>
public sealed record ReorgProfileValidator(List<IReorgProfileRule> Rules)
{
    public List<string> Validate(ReorgProfile item)
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

    public void RequireValid(ReorgProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgProfile> items)
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

/// <summary>Functions of reorg profile rules.</summary>
public static class ReorgProfileRules
{
    public static ReorgProfileValidator DefaultReorgProfileValidator()
    {
        return new ReorgProfileValidator(
            new List<IReorgProfileRule>
            {
                new ReorgProfileSequenceRangeRule(80, 255),
                new ReorgProfileRequiredLocationRule("reorg profile needs an entry"),
                new ReorgProfileLabelLengthRule(16),
            });
    }
}
