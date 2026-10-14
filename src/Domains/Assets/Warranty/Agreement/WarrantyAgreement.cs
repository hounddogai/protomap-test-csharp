namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty agreement is in its life cycle.</summary>
public enum WarrantyAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a warranty agreement and its current status.</summary>
public sealed class WarrantyAgreement(string code, string title, int quantity, string notes, double cost, bool active, string employeeId, double minimum, int version, string contactEmail, WarrantyAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Notes { get; set; } = notes;
    public double Cost { get; set; } = cost;
    public bool Active { get; set; } = active;
    public string EmployeeId { get; set; } = employeeId;
    public double Minimum { get; set; } = minimum;
    public int Version { get; set; } = version;
    public string ContactEmail { get; set; } = contactEmail;
    public WarrantyAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a warranty agreement in status idle.</summary>
    public static WarrantyAgreement NewWarrantyAgreement(string code, string title)
    {
        return new WarrantyAgreement(code, title, 0, "", 0.0, false, "", 0.0, 0, "", WarrantyAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyAgreementStatus next)
    {
        switch (Status)
        {
            case WarrantyAgreementStatus.Idle:
                switch (next)
                {
                    case WarrantyAgreementStatus.Warming:
                    case WarrantyAgreementStatus.Ready:
                        return true;
                }
                return false;
            case WarrantyAgreementStatus.Warming:
                switch (next)
                {
                    case WarrantyAgreementStatus.Ready:
                        return true;
                }
                return false;
            case WarrantyAgreementStatus.Ready:
                return next == WarrantyAgreementStatus.Stopped;
            case WarrantyAgreementStatus.Draining:
                return next == WarrantyAgreementStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WarrantyAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"warranty agreement"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Minimum + (double)Version, 2);
    }

    public WarrantyAgreement Copy()
    {
        return new WarrantyAgreement(
            Code,
            Title,
            Quantity,
            Notes,
            Cost,
            Active,
            EmployeeId,
            Minimum,
            Version,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
