namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal engagement group is in its life cycle.</summary>
public enum SeasonalEngagementGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal engagement group. Its code is unique within a tenant.</summary>
public sealed class SeasonalEngagementGroup(string code, string name, bool visible, int endsAt, int attempts, SeasonalEngagementGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public int EndsAt { get; set; } = endsAt;
    public int Attempts { get; set; } = attempts;
    public SeasonalEngagementGroupStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal engagement group in status requested.</summary>
    public static SeasonalEngagementGroup NewSeasonalEngagementGroup(string code, string name)
    {
        return new SeasonalEngagementGroup(code, name, false, 0, 0, SeasonalEngagementGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalEngagementGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalEngagementGroupStatus next)
    {
        switch (Status)
        {
            case SeasonalEngagementGroupStatus.Requested:
                return next == SeasonalEngagementGroupStatus.Revoked || next == SeasonalEngagementGroupStatus.Expired;
            case SeasonalEngagementGroupStatus.Granted:
                switch (next)
                {
                    case SeasonalEngagementGroupStatus.Revoked:
                    case SeasonalEngagementGroupStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalEngagementGroupStatus.Revoked:
                switch (next)
                {
                    case SeasonalEngagementGroupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalEngagementGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal engagement group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt - (double)Attempts, 2);
    }

    public SeasonalEngagementGroup Copy()
    {
        return new SeasonalEngagementGroup(Code, Name, Visible, EndsAt, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
