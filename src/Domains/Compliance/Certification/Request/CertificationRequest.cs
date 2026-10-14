namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification request is in its life cycle.</summary>
public enum CertificationRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A certification request as the certification team keeps it.</summary>
public sealed class CertificationRequest(string code, string title, string badge, string timezone, string employeeId, int quantity, CertificationRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public int Quantity { get; set; } = quantity;
    public CertificationRequestStatus Status { get; set; } = status;

    /// <summary>Starts a certification request in status open.</summary>
    public static CertificationRequest NewCertificationRequest(string code, string title)
    {
        return new CertificationRequest(code, title, "", "", "", 0, CertificationRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationRequestStatus next)
    {
        switch (Status)
        {
            case CertificationRequestStatus.Open:
                return next == CertificationRequestStatus.Reopened || next == CertificationRequestStatus.Archived;
            case CertificationRequestStatus.InReview:
                return next == CertificationRequestStatus.Reopened;
            case CertificationRequestStatus.Resolved:
                return next == CertificationRequestStatus.Archived;
            case CertificationRequestStatus.Reopened:
                switch (next)
                {
                    case CertificationRequestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public CertificationRequest Copy()
    {
        return new CertificationRequest(Code, Title, Badge, Timezone, EmployeeId, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
