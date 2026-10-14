namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk baseline is in its life cycle.</summary>
public enum DeskBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a desk baseline and its current status.</summary>
public sealed class DeskBaseline(string code, string title, int retries, string unit, int startsAt, DeskBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Unit { get; set; } = unit;
    public int StartsAt { get; set; } = startsAt;
    public DeskBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a desk baseline in status requested.</summary>
    public static DeskBaseline NewDeskBaseline(string code, string title)
    {
        return new DeskBaseline(code, title, 0, "", 0, DeskBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskBaselineStatus next)
    {
        switch (Status)
        {
            case DeskBaselineStatus.Requested:
                switch (next)
                {
                    case DeskBaselineStatus.Granted:
                    case DeskBaselineStatus.Revoked:
                        return true;
                }
                return false;
            case DeskBaselineStatus.Granted:
                switch (next)
                {
                    case DeskBaselineStatus.Revoked:
                    case DeskBaselineStatus.Expired:
                        return true;
                }
                return false;
            case DeskBaselineStatus.Revoked:
                return next == DeskBaselineStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DeskBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt + (double)Retries, 2);
    }

    public DeskBaseline Copy()
    {
        return new DeskBaseline(Code, Title, Retries, Unit, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
