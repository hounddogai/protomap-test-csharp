namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment snapshot is in its life cycle.</summary>
public enum GarnishmentSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A garnishment snapshot as the garnishment team keeps it.</summary>
public sealed class GarnishmentSnapshot(string code, string title, int endsAt, int revision, string region, GarnishmentSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public int Revision { get; set; } = revision;
    public string Region { get; set; } = region;
    public GarnishmentSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment snapshot in status draft.</summary>
    public static GarnishmentSnapshot NewGarnishmentSnapshot(string code, string title)
    {
        return new GarnishmentSnapshot(code, title, 0, 0, "", GarnishmentSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentSnapshotStatus next)
    {
        switch (Status)
        {
            case GarnishmentSnapshotStatus.Draft:
                return next == GarnishmentSnapshotStatus.Pending;
            case GarnishmentSnapshotStatus.Pending:
                return next == GarnishmentSnapshotStatus.Active;
            case GarnishmentSnapshotStatus.Active:
                switch (next)
                {
                    case GarnishmentSnapshotStatus.Suspended:
                    case GarnishmentSnapshotStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentSnapshotStatus.Suspended:
                switch (next)
                {
                    case GarnishmentSnapshotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Revision * (double)EndsAt, 2);
    }

    public GarnishmentSnapshot Copy()
    {
        return new GarnishmentSnapshot(Code, Title, EndsAt, Revision, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
