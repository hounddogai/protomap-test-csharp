namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance forecast is in its life cycle.</summary>
public enum BalanceForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A balance forecast. Its code is unique within a tenant.</summary>
public sealed class BalanceForecast(string code, string name, string region, int version, string phone, string notes, string ownerId, string grade, double margin, string segment, double ratio, BalanceForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int Version { get; set; } = version;
    public string Phone { get; set; } = phone;
    public string Notes { get; set; } = notes;
    public string OwnerId { get; set; } = ownerId;
    public string Grade { get; set; } = grade;
    public double Margin { get; set; } = margin;
    public string Segment { get; set; } = segment;
    public double Ratio { get; set; } = ratio;
    public BalanceForecastStatus Status { get; set; } = status;

    /// <summary>Starts a balance forecast in status proposed.</summary>
    public static BalanceForecast NewBalanceForecast(string code, string name)
    {
        return new BalanceForecast(code, name, "", 0, "", "", "", "", 0.0, "", 0.0, BalanceForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceForecastStatus next)
    {
        switch (Status)
        {
            case BalanceForecastStatus.Proposed:
                switch (next)
                {
                    case BalanceForecastStatus.Approved:
                    case BalanceForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case BalanceForecastStatus.Approved:
                switch (next)
                {
                    case BalanceForecastStatus.Rejected:
                    case BalanceForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case BalanceForecastStatus.Rejected:
                return next == BalanceForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BalanceForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Version * Margin, 2);
    }

    public BalanceForecast Copy()
    {
        return new BalanceForecast(
            Code,
            Name,
            Region,
            Version,
            Phone,
            Notes,
            OwnerId,
            Grade,
            Margin,
            Segment,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
