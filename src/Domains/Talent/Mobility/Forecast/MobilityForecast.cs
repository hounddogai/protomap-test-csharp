namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility forecast is in its life cycle.</summary>
public enum MobilityForecastStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A mobility forecast as the mobility team keeps it.</summary>
public sealed class MobilityForecast(string code, string name, int quantity, string contactEmail, string currency, double amount, double hours, string costCenter, string phone, double weight, string employeeId, MobilityForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public string Currency { get; set; } = currency;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public string CostCenter { get; set; } = costCenter;
    public string Phone { get; set; } = phone;
    public double Weight { get; set; } = weight;
    public string EmployeeId { get; set; } = employeeId;
    public MobilityForecastStatus Status { get; set; } = status;

    /// <summary>Starts a mobility forecast in status new.</summary>
    public static MobilityForecast NewMobilityForecast(string code, string name)
    {
        return new MobilityForecast(code, name, 0, "", "", 0.0, 0.0, "", "", 0.0, "", MobilityForecastStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityForecastStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityForecastStatus next)
    {
        switch (Status)
        {
            case MobilityForecastStatus.New:
                switch (next)
                {
                    case MobilityForecastStatus.Failed:
                        return true;
                }
                return false;
            case MobilityForecastStatus.Queued:
                switch (next)
                {
                    case MobilityForecastStatus.Done:
                    case MobilityForecastStatus.Failed:
                        return true;
                }
                return false;
            case MobilityForecastStatus.Running:
                return next == MobilityForecastStatus.Done;
            case MobilityForecastStatus.Done:
                return next == MobilityForecastStatus.Failed;
        }
        return false;
    }

    public void MoveTo(MobilityForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Weight - (double)Quantity, 2);
    }

    public MobilityForecast Copy()
    {
        return new MobilityForecast(
            Code,
            Name,
            Quantity,
            ContactEmail,
            Currency,
            Amount,
            Hours,
            CostCenter,
            Phone,
            Weight,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
