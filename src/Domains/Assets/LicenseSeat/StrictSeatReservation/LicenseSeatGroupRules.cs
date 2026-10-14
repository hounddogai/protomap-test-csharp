namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>A rule a license seat group must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatGroupRule
{
    List<string> Check(LicenseSeatGroup item);
}

public sealed record LicenseSeatGroupLabelLengthRule(int MaxLength) : ILicenseSeatGroupRule
{
    public List<string> Check(LicenseSeatGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatGroupRequiredLocationRule(string Message) : ILicenseSeatGroupRule
{
    public List<string> Check(LicenseSeatGroup item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatGroupCreatedAtRangeRule(int Min, int Max) : ILicenseSeatGroupRule
{
    public List<string> Check(LicenseSeatGroup item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatGroupCodeLengthRule(int MaxLength) : ILicenseSeatGroupRule
{
    public List<string> Check(LicenseSeatGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat group.</summary>
public sealed record LicenseSeatGroupValidator(List<ILicenseSeatGroupRule> Rules)
{
    public List<string> Validate(LicenseSeatGroup item)
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

    public void RequireValid(LicenseSeatGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatGroup> items)
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

/// <summary>Functions of license seat group rules.</summary>
public static class LicenseSeatGroupRules
{
    public static LicenseSeatGroupValidator DefaultLicenseSeatGroupValidator()
    {
        return new LicenseSeatGroupValidator(
            new List<ILicenseSeatGroupRule>
            {
                new LicenseSeatGroupLabelLengthRule(255),
                new LicenseSeatGroupRequiredLocationRule("license seat group needs a value"),
                new LicenseSeatGroupCreatedAtRangeRule(64, 64),
                new LicenseSeatGroupCodeLengthRule(32),
            });
    }
}
