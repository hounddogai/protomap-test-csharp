namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted refresh reservation is in its life cycle.</summary>
public enum WeightedRefreshReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A weighted refresh reservation as the refresh team keeps it.</summary>
public sealed class WeightedRefreshReservation(string code, string label, string externalRef, string team, string name, string badge, bool visible, string ownerId, string summary, double amount, WeightedRefreshReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Team { get; set; } = team;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public bool Visible { get; set; } = visible;
    public string OwnerId { get; set; } = ownerId;
    public string Summary { get; set; } = summary;
    public double Amount { get; set; } = amount;
    public WeightedRefreshReservationStatus Status { get; set; } = status;

    /// <summary>Starts a weighted refresh reservation in status draft.</summary>
    public static WeightedRefreshReservation NewWeightedRefreshReservation(string code, string label)
    {
        return new WeightedRefreshReservation(
            code,
            label,
            "",
            "",
            "",
            "",
            false,
            "",
            "",
            0.0,
            WeightedRefreshReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRefreshReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRefreshReservationStatus next)
    {
        switch (Status)
        {
            case WeightedRefreshReservationStatus.Draft:
                switch (next)
                {
                    case WeightedRefreshReservationStatus.Pending:
                        return true;
                }
                return false;
            case WeightedRefreshReservationStatus.Pending:
                switch (next)
                {
                    case WeightedRefreshReservationStatus.Suspended:
                    case WeightedRefreshReservationStatus.Closed:
                        return true;
                }
                return false;
            case WeightedRefreshReservationStatus.Active:
                return next == WeightedRefreshReservationStatus.Closed;
            case WeightedRefreshReservationStatus.Suspended:
                switch (next)
                {
                    case WeightedRefreshReservationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedRefreshReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted refresh reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public WeightedRefreshReservation Copy()
    {
        return new WeightedRefreshReservation(
            Code,
            Label,
            ExternalRef,
            Team,
            Name,
            Badge,
            Visible,
            OwnerId,
            Summary,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
