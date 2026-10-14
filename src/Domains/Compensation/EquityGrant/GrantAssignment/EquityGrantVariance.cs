namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant variance is in its life cycle.</summary>
public enum EquityGrantVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a equity grant variance and its current status.</summary>
public sealed class EquityGrantVariance(string code, string title, List<string> tags, double budget, double ratio, string name, string country, double maximum, string unit, EquityGrantVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public double Budget { get; set; } = budget;
    public double Ratio { get; set; } = ratio;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Maximum { get; set; } = maximum;
    public string Unit { get; set; } = unit;
    public EquityGrantVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant variance in status open.</summary>
    public static EquityGrantVariance NewEquityGrantVariance(string code, string title)
    {
        return new EquityGrantVariance(
            code,
            title,
            new List<string>(),
            0.0,
            0.0,
            "",
            "",
            0.0,
            "",
            EquityGrantVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantVarianceStatus next)
    {
        switch (Status)
        {
            case EquityGrantVarianceStatus.Open:
                return next == EquityGrantVarianceStatus.Resolved;
            case EquityGrantVarianceStatus.InReview:
                switch (next)
                {
                    case EquityGrantVarianceStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantVarianceStatus.Resolved:
                switch (next)
                {
                    case EquityGrantVarianceStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantVarianceStatus.Reopened:
                switch (next)
                {
                    case EquityGrantVarianceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetTotal()
    {
        return Math.Round(Budget + Maximum, 2);
    }

    public EquityGrantVariance Copy()
    {
        return new EquityGrantVariance(Code, Title, Tags, Budget, Ratio, Name, Country, Maximum, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
