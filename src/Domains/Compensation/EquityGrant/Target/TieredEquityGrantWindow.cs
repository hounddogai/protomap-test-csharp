namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered equity grant window is in its life cycle.</summary>
public enum TieredEquityGrantWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered equity grant window. Its code is unique within a tenant.</summary>
public sealed class TieredEquityGrantWindow(string code, string name, string country, int revision, string managerId, int tenureMonths, TieredEquityGrantWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int Revision { get; set; } = revision;
    public string ManagerId { get; set; } = managerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public TieredEquityGrantWindowStatus Status { get; set; } = status;

    /// <summary>Starts a tiered equity grant window in status requested.</summary>
    public static TieredEquityGrantWindow NewTieredEquityGrantWindow(string code, string name)
    {
        return new TieredEquityGrantWindow(code, name, "", 0, "", 0, TieredEquityGrantWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredEquityGrantWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredEquityGrantWindowStatus next)
    {
        switch (Status)
        {
            case TieredEquityGrantWindowStatus.Requested:
                switch (next)
                {
                    case TieredEquityGrantWindowStatus.Revoked:
                    case TieredEquityGrantWindowStatus.Expired:
                        return true;
                }
                return false;
            case TieredEquityGrantWindowStatus.Granted:
                switch (next)
                {
                    case TieredEquityGrantWindowStatus.Revoked:
                    case TieredEquityGrantWindowStatus.Expired:
                        return true;
                }
                return false;
            case TieredEquityGrantWindowStatus.Revoked:
                return next == TieredEquityGrantWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredEquityGrantWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered equity grant window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)TenureMonths + (double)Revision, 2);
    }

    public TieredEquityGrantWindow Copy()
    {
        return new TieredEquityGrantWindow(Code, Name, Country, Revision, ManagerId, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
