namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict vendor request is in its life cycle.</summary>
public enum StrictVendorRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict vendor request as the vendor team keeps it.</summary>
public sealed class StrictVendorRequest(string code, string title, int attempts, double margin, string jobCode, string externalRef, int createdAt, double ratio, double threshold, StrictVendorRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public double Margin { get; set; } = margin;
    public string JobCode { get; set; } = jobCode;
    public string ExternalRef { get; set; } = externalRef;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public double Threshold { get; set; } = threshold;
    public StrictVendorRequestStatus Status { get; set; } = status;

    /// <summary>Starts a strict vendor request in status proposed.</summary>
    public static StrictVendorRequest NewStrictVendorRequest(string code, string title)
    {
        return new StrictVendorRequest(code, title, 0, 0.0, "", "", 0, 0.0, 0.0, StrictVendorRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictVendorRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictVendorRequestStatus next)
    {
        switch (Status)
        {
            case StrictVendorRequestStatus.Proposed:
                switch (next)
                {
                    case StrictVendorRequestStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictVendorRequestStatus.Approved:
                switch (next)
                {
                    case StrictVendorRequestStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictVendorRequestStatus.Rejected:
                switch (next)
                {
                    case StrictVendorRequestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictVendorRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict vendor request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Attempts + Threshold, 2);
    }

    public StrictVendorRequest Copy()
    {
        return new StrictVendorRequest(
            Code,
            Title,
            Attempts,
            Margin,
            JobCode,
            ExternalRef,
            CreatedAt,
            Ratio,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
