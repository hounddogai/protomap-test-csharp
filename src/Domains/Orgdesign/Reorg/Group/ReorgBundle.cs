namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg bundle is in its life cycle.</summary>
public enum ReorgBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A reorg bundle. Its code is unique within a tenant.</summary>
public sealed class ReorgBundle(string code, string title, string team, string channel, string summary, string category, int quantity, ReorgBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Channel { get; set; } = channel;
    public string Summary { get; set; } = summary;
    public string Category { get; set; } = category;
    public int Quantity { get; set; } = quantity;
    public ReorgBundleStatus Status { get; set; } = status;

    /// <summary>Starts a reorg bundle in status draft.</summary>
    public static ReorgBundle NewReorgBundle(string code, string title)
    {
        return new ReorgBundle(code, title, "", "", "", "", 0, ReorgBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgBundleStatus next)
    {
        switch (Status)
        {
            case ReorgBundleStatus.Draft:
                return next == ReorgBundleStatus.Pending || next == ReorgBundleStatus.Closed;
            case ReorgBundleStatus.Pending:
                return next == ReorgBundleStatus.Active;
            case ReorgBundleStatus.Active:
                return next == ReorgBundleStatus.Suspended;
            case ReorgBundleStatus.Suspended:
                return next == ReorgBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ReorgBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public ReorgBundle Copy()
    {
        return new ReorgBundle(Code, Title, Team, Channel, Summary, Category, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
