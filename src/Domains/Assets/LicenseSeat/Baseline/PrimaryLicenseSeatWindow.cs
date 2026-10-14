namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary license seat window is in its life cycle.</summary>
public enum PrimaryLicenseSeatWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary license seat window. Its code is unique within a tenant.</summary>
public sealed class PrimaryLicenseSeatWindow(string code, string label, int retries, string location, int endsAt, double rating, string costCenter, string employeeId, int leaveDays, double amount, string currency, PrimaryLicenseSeatWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public string Location { get; set; } = location;
    public int EndsAt { get; set; } = endsAt;
    public double Rating { get; set; } = rating;
    public string CostCenter { get; set; } = costCenter;
    public string EmployeeId { get; set; } = employeeId;
    public int LeaveDays { get; set; } = leaveDays;
    public double Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
    public PrimaryLicenseSeatWindowStatus Status { get; set; } = status;

    /// <summary>Starts a primary license seat window in status idle.</summary>
    public static PrimaryLicenseSeatWindow NewPrimaryLicenseSeatWindow(string code, string label)
    {
        return new PrimaryLicenseSeatWindow(
            code,
            label,
            0,
            "",
            0,
            0.0,
            "",
            "",
            0,
            0.0,
            "",
            PrimaryLicenseSeatWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLicenseSeatWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLicenseSeatWindowStatus next)
    {
        switch (Status)
        {
            case PrimaryLicenseSeatWindowStatus.Idle:
                switch (next)
                {
                    case PrimaryLicenseSeatWindowStatus.Ready:
                        return true;
                }
                return false;
            case PrimaryLicenseSeatWindowStatus.Warming:
                switch (next)
                {
                    case PrimaryLicenseSeatWindowStatus.Draining:
                    case PrimaryLicenseSeatWindowStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryLicenseSeatWindowStatus.Ready:
                switch (next)
                {
                    case PrimaryLicenseSeatWindowStatus.Draining:
                    case PrimaryLicenseSeatWindowStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryLicenseSeatWindowStatus.Draining:
                switch (next)
                {
                    case PrimaryLicenseSeatWindowStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryLicenseSeatWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary license seat window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetValue()
    {
        return Math.Round((double)LeaveDays + Rating, 2);
    }

    public PrimaryLicenseSeatWindow Copy()
    {
        return new PrimaryLicenseSeatWindow(
            Code,
            Label,
            Retries,
            Location,
            EndsAt,
            Rating,
            CostCenter,
            EmployeeId,
            LeaveDays,
            Amount,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
