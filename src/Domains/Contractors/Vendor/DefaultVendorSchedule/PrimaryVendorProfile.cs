namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary vendor profile is in its life cycle.</summary>
public enum PrimaryVendorProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a primary vendor profile and its current status.</summary>
public sealed class PrimaryVendorProfile(string code, string title, double floorValue, bool visible, string reviewer, PrimaryVendorProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public bool Visible { get; set; } = visible;
    public string Reviewer { get; set; } = reviewer;
    public PrimaryVendorProfileStatus Status { get; set; } = status;

    /// <summary>Starts a primary vendor profile in status created.</summary>
    public static PrimaryVendorProfile NewPrimaryVendorProfile(string code, string title)
    {
        return new PrimaryVendorProfile(code, title, 0.0, false, "", PrimaryVendorProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryVendorProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryVendorProfileStatus next)
    {
        switch (Status)
        {
            case PrimaryVendorProfileStatus.Created:
                return next == PrimaryVendorProfileStatus.Published || next == PrimaryVendorProfileStatus.Retired;
            case PrimaryVendorProfileStatus.Validated:
                switch (next)
                {
                    case PrimaryVendorProfileStatus.Published:
                        return true;
                }
                return false;
            case PrimaryVendorProfileStatus.Published:
                switch (next)
                {
                    case PrimaryVendorProfileStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryVendorProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary vendor profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public PrimaryVendorProfile Copy()
    {
        return new PrimaryVendorProfile(Code, Title, FloorValue, Visible, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
