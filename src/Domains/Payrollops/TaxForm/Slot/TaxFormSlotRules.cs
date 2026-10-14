namespace Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>A rule a tax form slot must pass. Check returns its problems, or none.</summary>
public interface ITaxFormSlotRule
{
    List<string> Check(TaxFormSlot item);
}

public sealed record TaxFormSlotCodeLengthRule(int MaxLength) : ITaxFormSlotRule
{
    public List<string> Check(TaxFormSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormSlotRetriesRangeRule(int Min, int Max) : ITaxFormSlotRule
{
    public List<string> Check(TaxFormSlot item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormSlotActiveGuardRule(double Limit) : ITaxFormSlotRule
{
    public List<string> Check(TaxFormSlot item)
    {
        if (item.Active && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormSlotBadgeLengthRule(int MaxLength) : ITaxFormSlotRule
{
    public List<string> Check(TaxFormSlot item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormSlotCeilingRangeRule(double Min, double Max) : ITaxFormSlotRule
{
    public List<string> Check(TaxFormSlot item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form slot.</summary>
public sealed record TaxFormSlotValidator(List<ITaxFormSlotRule> Rules)
{
    public List<string> Validate(TaxFormSlot item)
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

    public void RequireValid(TaxFormSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormSlot> items)
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

/// <summary>Functions of tax form slot rules.</summary>
public static class TaxFormSlotRules
{
    public static TaxFormSlotValidator DefaultTaxFormSlotValidator()
    {
        return new TaxFormSlotValidator(
            new List<ITaxFormSlotRule>
            {
                new TaxFormSlotCodeLengthRule(64),
                new TaxFormSlotRetriesRangeRule(32, 80),
                new TaxFormSlotActiveGuardRule(4.4),
                new TaxFormSlotBadgeLengthRule(255),
                new TaxFormSlotCeilingRangeRule(8.9, 4181.6),
            });
    }
}
