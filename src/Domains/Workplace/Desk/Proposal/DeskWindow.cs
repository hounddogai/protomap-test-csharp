namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk window is in its life cycle.</summary>
public enum DeskWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A desk window. Its code is unique within a tenant.</summary>
public sealed class DeskWindow(string code, string name, string currency, double maximum, bool visible, string team, DeskWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public double Maximum { get; set; } = maximum;
    public bool Visible { get; set; } = visible;
    public string Team { get; set; } = team;
    public DeskWindowStatus Status { get; set; } = status;

    /// <summary>Starts a desk window in status draft.</summary>
    public static DeskWindow NewDeskWindow(string code, string name)
    {
        return new DeskWindow(code, name, "", 0.0, false, "", DeskWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskWindowStatus next)
    {
        switch (Status)
        {
            case DeskWindowStatus.Draft:
                switch (next)
                {
                    case DeskWindowStatus.Pending:
                    case DeskWindowStatus.Closed:
                        return true;
                }
                return false;
            case DeskWindowStatus.Pending:
                switch (next)
                {
                    case DeskWindowStatus.Closed:
                        return true;
                }
                return false;
            case DeskWindowStatus.Active:
                switch (next)
                {
                    case DeskWindowStatus.Suspended:
                        return true;
                }
                return false;
            case DeskWindowStatus.Suspended:
                switch (next)
                {
                    case DeskWindowStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"desk window"} {Code}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public DeskWindow Copy()
    {
        return new DeskWindow(Code, Name, Currency, Maximum, Visible, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
