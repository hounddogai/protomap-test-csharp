namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling vendor variance is in its life cycle.</summary>
public enum RollingVendorVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling vendor variance as the vendor team keeps it.</summary>
public sealed class RollingVendorVariance(string code, string name, int retries, string description, string timezone, string channel, RollingVendorVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Description { get; set; } = description;
    public string Timezone { get; set; } = timezone;
    public string Channel { get; set; } = channel;
    public RollingVendorVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling vendor variance in status draft.</summary>
    public static RollingVendorVariance NewRollingVendorVariance(string code, string name)
    {
        return new RollingVendorVariance(code, name, 0, "", "", "", RollingVendorVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingVendorVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingVendorVarianceStatus next)
    {
        switch (Status)
        {
            case RollingVendorVarianceStatus.Draft:
                switch (next)
                {
                    case RollingVendorVarianceStatus.Active:
                    case RollingVendorVarianceStatus.Closed:
                        return true;
                }
                return false;
            case RollingVendorVarianceStatus.Pending:
                switch (next)
                {
                    case RollingVendorVarianceStatus.Suspended:
                        return true;
                }
                return false;
            case RollingVendorVarianceStatus.Active:
                switch (next)
                {
                    case RollingVendorVarianceStatus.Suspended:
                    case RollingVendorVarianceStatus.Closed:
                        return true;
                }
                return false;
            case RollingVendorVarianceStatus.Suspended:
                return next == RollingVendorVarianceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingVendorVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling vendor variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling vendor variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public RollingVendorVariance Copy()
    {
        return new RollingVendorVariance(Code, Name, Retries, Description, Timezone, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
