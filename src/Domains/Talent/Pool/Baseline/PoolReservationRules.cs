namespace Acme.Workforce.Talent.Pool.Baseline;

/// <summary>A rule a pool reservation must pass. Check returns its problems, or none.</summary>
public interface IPoolReservationRule
{
    List<string> Check(PoolReservation item);
}

public sealed record PoolReservationLabelLengthRule(int MaxLength) : IPoolReservationRule
{
    public List<string> Check(PoolReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolReservationRequiredLabelRule(string Message) : IPoolReservationRule
{
    public List<string> Check(PoolReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolReservationCodePrefixRule(string Prefix) : IPoolReservationRule
{
    public List<string> Check(PoolReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolReservationCodeLengthRule(int MaxLength) : IPoolReservationRule
{
    public List<string> Check(PoolReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolReservationCapacityRangeRule(int Min, int Max) : IPoolReservationRule
{
    public List<string> Check(PoolReservation item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool reservation.</summary>
public sealed record PoolReservationValidator(List<IPoolReservationRule> Rules)
{
    public List<string> Validate(PoolReservation item)
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

    public void RequireValid(PoolReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolReservation> items)
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

/// <summary>Functions of pool reservation rules.</summary>
public static class PoolReservationRules
{
    public static PoolReservationValidator DefaultPoolReservationValidator()
    {
        return new PoolReservationValidator(
            new List<IPoolReservationRule>
            {
                new PoolReservationLabelLengthRule(32),
                new PoolReservationRequiredLabelRule("pool reservation needs an entry"),
                new PoolReservationCodePrefixRule("REF"),
                new PoolReservationCodeLengthRule(80),
                new PoolReservationCapacityRangeRule(16, 255),
            });
    }
}
