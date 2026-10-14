namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional warranty exception is in its life cycle.</summary>
public enum RegionalWarrantyExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional warranty exception. Its code is unique within a tenant.</summary>
public sealed class RegionalWarrantyException(string code, string name, double cost, double amount, string currency, double rating, double margin, double ceiling, int revision, int retries, RegionalWarrantyExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public double Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
    public double Rating { get; set; } = rating;
    public double Margin { get; set; } = margin;
    public double Ceiling { get; set; } = ceiling;
    public int Revision { get; set; } = revision;
    public int Retries { get; set; } = retries;
    public RegionalWarrantyExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a regional warranty exception in status planned.</summary>
    public static RegionalWarrantyException NewRegionalWarrantyException(string code, string name)
    {
        return new RegionalWarrantyException(
            code,
            name,
            0.0,
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            0,
            0,
            RegionalWarrantyExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalWarrantyExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalWarrantyExceptionStatus next)
    {
        switch (Status)
        {
            case RegionalWarrantyExceptionStatus.Planned:
                return next == RegionalWarrantyExceptionStatus.Scheduled || next == RegionalWarrantyExceptionStatus.Completed;
            case RegionalWarrantyExceptionStatus.Scheduled:
                return next == RegionalWarrantyExceptionStatus.Completed;
            case RegionalWarrantyExceptionStatus.InProgress:
                switch (next)
                {
                    case RegionalWarrantyExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalWarrantyExceptionStatus.Completed:
                return next == RegionalWarrantyExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RegionalWarrantyExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional warranty exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Margin - Amount, 2);
    }

    public RegionalWarrantyException Copy()
    {
        return new RegionalWarrantyException(
            Code,
            Name,
            Cost,
            Amount,
            Currency,
            Rating,
            Margin,
            Ceiling,
            Revision,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
