namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor notice must pass. Check returns its problems, or none.</summary>
public interface IVisitorNoticeRule
{
    List<string> Check(VisitorNotice item);
}

public sealed record VisitorNoticeVisibleGuardRule(double Limit) : IVisitorNoticeRule
{
    public List<string> Check(VisitorNotice item)
    {
        if (item.Visible && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record VisitorNoticeDescriptionLengthRule(int MaxLength) : IVisitorNoticeRule
{
    public List<string> Check(VisitorNotice item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorNoticeNameLengthRule(int MaxLength) : IVisitorNoticeRule
{
    public List<string> Check(VisitorNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorNoticeAllowedDescriptionRule(List<string> Allowed) : IVisitorNoticeRule
{
    public List<string> Check(VisitorNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record VisitorNoticeGraceDaysRangeRule(int Min, int Max) : IVisitorNoticeRule
{
    public List<string> Check(VisitorNotice item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor notice.</summary>
public sealed record VisitorNoticeValidator(List<IVisitorNoticeRule> Rules)
{
    public List<string> Validate(VisitorNotice item)
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

    public void RequireValid(VisitorNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorNotice> items)
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

/// <summary>Functions of visitor notice rules.</summary>
public static class VisitorNoticeRules
{
    public static VisitorNoticeValidator DefaultVisitorNoticeValidator()
    {
        return new VisitorNoticeValidator(
            new List<IVisitorNoticeRule>
            {
                new VisitorNoticeVisibleGuardRule(6.9),
                new VisitorNoticeDescriptionLengthRule(64),
                new VisitorNoticeNameLengthRule(255),
                new VisitorNoticeAllowedDescriptionRule(new List<string>()),
                new VisitorNoticeGraceDaysRangeRule(64, 1000),
            });
    }
}
