namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary equity grant override is in its life cycle.</summary>
public enum PrimaryEquityGrantOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary equity grant override as the equity grant team keeps it.</summary>
public sealed class PrimaryEquityGrantOverride(string code, string title, string team, string checksum, string contactEmail, bool active, double score, PrimaryEquityGrantOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Checksum { get; set; } = checksum;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Active { get; set; } = active;
    public double Score { get; set; } = score;
    public PrimaryEquityGrantOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a primary equity grant override in status requested.</summary>
    public static PrimaryEquityGrantOverride NewPrimaryEquityGrantOverride(string code, string title)
    {
        return new PrimaryEquityGrantOverride(
            code,
            title,
            "",
            "",
            "",
            false,
            0.0,
            PrimaryEquityGrantOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryEquityGrantOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryEquityGrantOverrideStatus next)
    {
        switch (Status)
        {
            case PrimaryEquityGrantOverrideStatus.Requested:
                switch (next)
                {
                    case PrimaryEquityGrantOverrideStatus.Revoked:
                    case PrimaryEquityGrantOverrideStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryEquityGrantOverrideStatus.Granted:
                switch (next)
                {
                    case PrimaryEquityGrantOverrideStatus.Revoked:
                    case PrimaryEquityGrantOverrideStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryEquityGrantOverrideStatus.Revoked:
                switch (next)
                {
                    case PrimaryEquityGrantOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryEquityGrantOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary equity grant override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PrimaryEquityGrantOverride Copy()
    {
        return new PrimaryEquityGrantOverride(Code, Title, Team, Checksum, ContactEmail, Active, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
