namespace Acme.Workforce.Orgdesign.Position.Adjustment;

/// <summary>A rule a position snapshot must pass. Check returns its problems, or none.</summary>
public interface IPositionSnapshotRule
{
    List<string> Check(PositionSnapshot item);
}

public sealed record PositionSnapshotRatioRangeRule(double Min, double Max) : IPositionSnapshotRule
{
    public List<string> Check(PositionSnapshot item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSnapshotSummaryLengthRule(int MaxLength) : IPositionSnapshotRule
{
    public List<string> Check(PositionSnapshot item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSnapshotRequiredCodeRule(string Message) : IPositionSnapshotRule
{
    public List<string> Check(PositionSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionSnapshotCodeLengthRule(int MaxLength) : IPositionSnapshotRule
{
    public List<string> Check(PositionSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSnapshotLabelLengthRule(int MaxLength) : IPositionSnapshotRule
{
    public List<string> Check(PositionSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position snapshot.</summary>
public sealed record PositionSnapshotValidator(List<IPositionSnapshotRule> Rules)
{
    public List<string> Validate(PositionSnapshot item)
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

    public void RequireValid(PositionSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionSnapshot> items)
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

/// <summary>Functions of position snapshot rules.</summary>
public static class PositionSnapshotRules
{
    public static PositionSnapshotValidator DefaultPositionSnapshotValidator()
    {
        return new PositionSnapshotValidator(
            new List<IPositionSnapshotRule>
            {
                new PositionSnapshotRatioRangeRule(9.0, 4527.4),
                new PositionSnapshotSummaryLengthRule(86400),
                new PositionSnapshotRequiredCodeRule("position snapshot needs an entry"),
                new PositionSnapshotCodeLengthRule(120),
                new PositionSnapshotLabelLengthRule(255),
            });
    }
}
