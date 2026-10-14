namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default vendor variance is in its life cycle.</summary>
public enum DefaultVendorVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default vendor variance. Its code is unique within a tenant.</summary>
public sealed class DefaultVendorVariance(string code, string title, double rate, bool visible, string category, string phone, bool pinned, string department, int windowDays, string grade, DefaultVendorVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public bool Visible { get; set; } = visible;
    public string Category { get; set; } = category;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public string Department { get; set; } = department;
    public int WindowDays { get; set; } = windowDays;
    public string Grade { get; set; } = grade;
    public DefaultVendorVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a default vendor variance in status draft.</summary>
    public static DefaultVendorVariance NewDefaultVendorVariance(string code, string title)
    {
        return new DefaultVendorVariance(
            code,
            title,
            0.0,
            false,
            "",
            "",
            false,
            "",
            0,
            "",
            DefaultVendorVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultVendorVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultVendorVarianceStatus next)
    {
        switch (Status)
        {
            case DefaultVendorVarianceStatus.Draft:
                switch (next)
                {
                    case DefaultVendorVarianceStatus.Pending:
                        return true;
                }
                return false;
            case DefaultVendorVarianceStatus.Pending:
                switch (next)
                {
                    case DefaultVendorVarianceStatus.Suspended:
                    case DefaultVendorVarianceStatus.Closed:
                        return true;
                }
                return false;
            case DefaultVendorVarianceStatus.Active:
                switch (next)
                {
                    case DefaultVendorVarianceStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultVendorVarianceStatus.Suspended:
                return next == DefaultVendorVarianceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultVendorVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default vendor variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)WindowDays + Rate, 2);
    }

    public DefaultVendorVariance Copy()
    {
        return new DefaultVendorVariance(
            Code,
            Title,
            Rate,
            Visible,
            Category,
            Phone,
            Pinned,
            Department,
            WindowDays,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
