namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop target is in its life cycle.</summary>
public enum LaptopTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a laptop target and its current status.</summary>
public sealed class LaptopTarget(string code, string label, string locale, string contactEmail, double weight, int batchSize, string costCenter, string country, double rate, double fte, string timezone, LaptopTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string ContactEmail { get; set; } = contactEmail;
    public double Weight { get; set; } = weight;
    public int BatchSize { get; set; } = batchSize;
    public string CostCenter { get; set; } = costCenter;
    public string Country { get; set; } = country;
    public double Rate { get; set; } = rate;
    public double Fte { get; set; } = fte;
    public string Timezone { get; set; } = timezone;
    public LaptopTargetStatus Status { get; set; } = status;

    /// <summary>Starts a laptop target in status planned.</summary>
    public static LaptopTarget NewLaptopTarget(string code, string label)
    {
        return new LaptopTarget(code, label, "", "", 0.0, 0, "", "", 0.0, 0.0, "", LaptopTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopTargetStatus next)
    {
        switch (Status)
        {
            case LaptopTargetStatus.Planned:
                switch (next)
                {
                    case LaptopTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopTargetStatus.Scheduled:
                switch (next)
                {
                    case LaptopTargetStatus.Completed:
                    case LaptopTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopTargetStatus.InProgress:
                switch (next)
                {
                    case LaptopTargetStatus.Completed:
                    case LaptopTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopTargetStatus.Completed:
                switch (next)
                {
                    case LaptopTargetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Fte * (double)BatchSize, 2);
    }

    public LaptopTarget Copy()
    {
        return new LaptopTarget(
            Code,
            Label,
            Locale,
            ContactEmail,
            Weight,
            BatchSize,
            CostCenter,
            Country,
            Rate,
            Fte,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
