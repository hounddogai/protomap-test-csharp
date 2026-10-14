namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default position quota is in its life cycle.</summary>
public enum DefaultPositionQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A default position quota as the position team keeps it.</summary>
public sealed class DefaultPositionQuota(string code, string label, bool visible, string location, List<string> tags, string team, double hours, int tenureMonths, DefaultPositionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Location { get; set; } = location;
    public List<string> Tags { get; set; } = tags;
    public string Team { get; set; } = team;
    public double Hours { get; set; } = hours;
    public int TenureMonths { get; set; } = tenureMonths;
    public DefaultPositionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a default position quota in status open.</summary>
    public static DefaultPositionQuota NewDefaultPositionQuota(string code, string label)
    {
        return new DefaultPositionQuota(
            code,
            label,
            false,
            "",
            new List<string>(),
            "",
            0.0,
            0,
            DefaultPositionQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPositionQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPositionQuotaStatus next)
    {
        switch (Status)
        {
            case DefaultPositionQuotaStatus.Open:
                switch (next)
                {
                    case DefaultPositionQuotaStatus.Resolved:
                    case DefaultPositionQuotaStatus.Archived:
                        return true;
                }
                return false;
            case DefaultPositionQuotaStatus.InReview:
                switch (next)
                {
                    case DefaultPositionQuotaStatus.Resolved:
                    case DefaultPositionQuotaStatus.Archived:
                        return true;
                }
                return false;
            case DefaultPositionQuotaStatus.Resolved:
                return next == DefaultPositionQuotaStatus.Archived;
            case DefaultPositionQuotaStatus.Reopened:
                return next == DefaultPositionQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DefaultPositionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default position quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double WeightedLoad()
    {
        return Math.Round(Hours * (double)TenureMonths, 2);
    }

    public DefaultPositionQuota Copy()
    {
        return new DefaultPositionQuota(Code, Label, Visible, Location, Tags, Team, Hours, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
