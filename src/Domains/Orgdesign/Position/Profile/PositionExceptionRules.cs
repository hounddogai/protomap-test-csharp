namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position exception must pass. Check returns its problems, or none.</summary>
public interface IPositionExceptionRule
{
    List<string> Check(PositionException item);
}

public sealed record PositionExceptionRequiredChecksumRule(string Message) : IPositionExceptionRule
{
    public List<string> Check(PositionException item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionExceptionTitleLengthRule(int MaxLength) : IPositionExceptionRule
{
    public List<string> Check(PositionException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionExceptionCodePrefixRule(string Prefix) : IPositionExceptionRule
{
    public List<string> Check(PositionException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionExceptionCeilingRangeRule(double Min, double Max) : IPositionExceptionRule
{
    public List<string> Check(PositionException item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionExceptionRequiredTitleRule(string Message) : IPositionExceptionRule
{
    public List<string> Check(PositionException item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionExceptionAllowedTitleRule(List<string> Allowed) : IPositionExceptionRule
{
    public List<string> Check(PositionException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position exception.</summary>
public sealed record PositionExceptionValidator(List<IPositionExceptionRule> Rules)
{
    public List<string> Validate(PositionException item)
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

    public void RequireValid(PositionException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionException> items)
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

/// <summary>Functions of position exception rules.</summary>
public static class PositionExceptionRules
{
    public static PositionExceptionValidator DefaultPositionExceptionValidator()
    {
        return new PositionExceptionValidator(
            new List<IPositionExceptionRule>
            {
                new PositionExceptionRequiredChecksumRule("position exception needs an entry"),
                new PositionExceptionTitleLengthRule(120),
                new PositionExceptionCodePrefixRule("REF"),
                new PositionExceptionCeilingRangeRule(4.4, 1954.0),
                new PositionExceptionRequiredTitleRule("position exception needs this field"),
                new PositionExceptionAllowedTitleRule(new List<string>()),
            });
    }
}
