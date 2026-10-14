namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position entry must pass. Check returns its problems, or none.</summary>
public interface IPositionEntryRule
{
    List<string> Check(PositionEntry item);
}

public sealed record PositionEntryFteRangeRule(double Min, double Max) : IPositionEntryRule
{
    public List<string> Check(PositionEntry item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionEntryCurrencyLengthRule(int MaxLength) : IPositionEntryRule
{
    public List<string> Check(PositionEntry item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionEntryAllowedCurrencyRule(List<string> Allowed) : IPositionEntryRule
{
    public List<string> Check(PositionEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PositionEntryRequiredCurrencyRule(string Message) : IPositionEntryRule
{
    public List<string> Check(PositionEntry item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionEntryOwnerIdLengthRule(int MaxLength) : IPositionEntryRule
{
    public List<string> Check(PositionEntry item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position entry.</summary>
public sealed record PositionEntryValidator(List<IPositionEntryRule> Rules)
{
    public List<string> Validate(PositionEntry item)
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

    public void RequireValid(PositionEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionEntry> items)
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

/// <summary>Functions of position entry rules.</summary>
public static class PositionEntryRules
{
    public static PositionEntryValidator DefaultPositionEntryValidator()
    {
        return new PositionEntryValidator(
            new List<IPositionEntryRule>
            {
                new PositionEntryFteRangeRule(4.0, 2456.3),
                new PositionEntryCurrencyLengthRule(16),
                new PositionEntryAllowedCurrencyRule(new List<string> { "regional" }),
                new PositionEntryRequiredCurrencyRule("position entry needs a value"),
                new PositionEntryOwnerIdLengthRule(120),
            });
    }
}
