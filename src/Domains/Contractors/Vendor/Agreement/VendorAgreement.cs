namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor agreement is in its life cycle.</summary>
public enum VendorAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One vendor agreement, with what the planners need to act on it.</summary>
public sealed class VendorAgreement(string code, string name, double score, string ownerId, string country, double maximum, VendorAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string OwnerId { get; set; } = ownerId;
    public string Country { get; set; } = country;
    public double Maximum { get; set; } = maximum;
    public VendorAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a vendor agreement in status planned.</summary>
    public static VendorAgreement NewVendorAgreement(string code, string name)
    {
        return new VendorAgreement(code, name, 0.0, "", "", 0.0, VendorAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorAgreementStatus next)
    {
        switch (Status)
        {
            case VendorAgreementStatus.Planned:
                switch (next)
                {
                    case VendorAgreementStatus.InProgress:
                        return true;
                }
                return false;
            case VendorAgreementStatus.Scheduled:
                return next == VendorAgreementStatus.InProgress || next == VendorAgreementStatus.Completed;
            case VendorAgreementStatus.InProgress:
                switch (next)
                {
                    case VendorAgreementStatus.Cancelled:
                        return true;
                }
                return false;
            case VendorAgreementStatus.Completed:
                switch (next)
                {
                    case VendorAgreementStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"vendor agreement"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Score - Maximum, 2);
    }

    public VendorAgreement Copy()
    {
        return new VendorAgreement(Code, Name, Score, OwnerId, Country, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
