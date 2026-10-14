namespace Acme.Workforce.Assets.Warranty.Template;

/// <summary>A rule a warranty record must pass. Check returns its problems, or none.</summary>
public interface IWarrantyRecordRule
{
    List<string> Check(WarrantyRecord item);
}

public sealed record WarrantyRecordRequiredCodeRule(string Message) : IWarrantyRecordRule
{
    public List<string> Check(WarrantyRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyRecordCeilingRangeRule(double Min, double Max) : IWarrantyRecordRule
{
    public List<string> Check(WarrantyRecord item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyRecordContactEmailLengthRule(int MaxLength) : IWarrantyRecordRule
{
    public List<string> Check(WarrantyRecord item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyRecordCodeLengthRule(int MaxLength) : IWarrantyRecordRule
{
    public List<string> Check(WarrantyRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyRecordRequiredTitleRule(string Message) : IWarrantyRecordRule
{
    public List<string> Check(WarrantyRecord item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty record.</summary>
public sealed record WarrantyRecordValidator(List<IWarrantyRecordRule> Rules)
{
    public List<string> Validate(WarrantyRecord item)
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

    public void RequireValid(WarrantyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyRecord> items)
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

/// <summary>Functions of warranty record rules.</summary>
public static class WarrantyRecordRules
{
    public static WarrantyRecordValidator DefaultWarrantyRecordValidator()
    {
        return new WarrantyRecordValidator(
            new List<IWarrantyRecordRule>
            {
                new WarrantyRecordRequiredCodeRule("warranty record needs an entry"),
                new WarrantyRecordCeilingRangeRule(2.9, 2228.8),
                new WarrantyRecordContactEmailLengthRule(120),
                new WarrantyRecordCodeLengthRule(120),
                new WarrantyRecordRequiredTitleRule("warranty record needs an entry"),
            });
    }
}
