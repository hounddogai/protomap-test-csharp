namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling pool ledger must pass. Check returns its problems, or none.</summary>
public interface IRollingPoolLedgerRule
{
    List<string> Check(RollingPoolLedger item);
}

public sealed record RollingPoolLedgerAllowedCodeRule(List<string> Allowed) : IRollingPoolLedgerRule
{
    public List<string> Check(RollingPoolLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolLedgerRateRangeRule(double Min, double Max) : IRollingPoolLedgerRule
{
    public List<string> Check(RollingPoolLedger item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolLedgerCodeLengthRule(int MaxLength) : IRollingPoolLedgerRule
{
    public List<string> Check(RollingPoolLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolLedgerLabelLengthRule(int MaxLength) : IRollingPoolLedgerRule
{
    public List<string> Check(RollingPoolLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pool ledger.</summary>
public sealed record RollingPoolLedgerValidator(List<IRollingPoolLedgerRule> Rules)
{
    public List<string> Validate(RollingPoolLedger item)
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

    public void RequireValid(RollingPoolLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPoolLedger> items)
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

/// <summary>Functions of rolling pool ledger rules.</summary>
public static class RollingPoolLedgerRules
{
    public static RollingPoolLedgerValidator DefaultRollingPoolLedgerValidator()
    {
        return new RollingPoolLedgerValidator(
            new List<IRollingPoolLedgerRule>
            {
                new RollingPoolLedgerAllowedCodeRule(new List<string>()),
                new RollingPoolLedgerRateRangeRule(8.4, 4781.9),
                new RollingPoolLedgerCodeLengthRule(80),
                new RollingPoolLedgerLabelLengthRule(16),
            });
    }
}
