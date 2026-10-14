namespace Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>A rule a position ledger must pass. Check returns its problems, or none.</summary>
public interface IPositionLedgerRule
{
    List<string> Check(PositionLedger item);
}

public sealed record PositionLedgerRegionLengthRule(int MaxLength) : IPositionLedgerRule
{
    public List<string> Check(PositionLedger item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionLedgerAttemptsRangeRule(int Min, int Max) : IPositionLedgerRule
{
    public List<string> Check(PositionLedger item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionLedgerRequiredCodeRule(string Message) : IPositionLedgerRule
{
    public List<string> Check(PositionLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position ledger.</summary>
public sealed record PositionLedgerValidator(List<IPositionLedgerRule> Rules)
{
    public List<string> Validate(PositionLedger item)
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

    public void RequireValid(PositionLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionLedger> items)
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

/// <summary>Functions of position ledger rules.</summary>
public static class PositionLedgerRules
{
    public static PositionLedgerValidator DefaultPositionLedgerValidator()
    {
        return new PositionLedgerValidator(
            new List<IPositionLedgerRule>
            {
                new PositionLedgerRegionLengthRule(16),
                new PositionLedgerAttemptsRangeRule(120, 1000),
                new PositionLedgerRequiredCodeRule("position ledger needs a value"),
            });
    }
}
