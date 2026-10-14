namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal warranty profile is in its life cycle.</summary>
public enum SeasonalWarrantyProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a seasonal warranty profile and its current status.</summary>
public sealed class SeasonalWarrantyProfile(string code, string title, string ownerId, string team, double margin, string label, double score, int limit, SeasonalWarrantyProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Team { get; set; } = team;
    public double Margin { get; set; } = margin;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public int Limit { get; set; } = limit;
    public SeasonalWarrantyProfileStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal warranty profile in status requested.</summary>
    public static SeasonalWarrantyProfile NewSeasonalWarrantyProfile(string code, string title)
    {
        return new SeasonalWarrantyProfile(
            code,
            title,
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            SeasonalWarrantyProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalWarrantyProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalWarrantyProfileStatus next)
    {
        switch (Status)
        {
            case SeasonalWarrantyProfileStatus.Requested:
                switch (next)
                {
                    case SeasonalWarrantyProfileStatus.Granted:
                        return true;
                }
                return false;
            case SeasonalWarrantyProfileStatus.Granted:
                switch (next)
                {
                    case SeasonalWarrantyProfileStatus.Revoked:
                    case SeasonalWarrantyProfileStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalWarrantyProfileStatus.Revoked:
                return next == SeasonalWarrantyProfileStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalWarrantyProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal warranty profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Margin - (double)Limit, 2);
    }

    public SeasonalWarrantyProfile Copy()
    {
        return new SeasonalWarrantyProfile(Code, Title, OwnerId, Team, Margin, Label, Score, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
