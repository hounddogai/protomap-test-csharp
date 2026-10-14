namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band audit is in its life cycle.</summary>
public enum PayBandAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pay band audit, with what the planners need to act on it.</summary>
public sealed class PayBandAudit(string code, string label, string summary, double weight, string description, string contactEmail, double fte, string department, string grade, string title, int retries, PayBandAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public string Description { get; set; } = description;
    public string ContactEmail { get; set; } = contactEmail;
    public double Fte { get; set; } = fte;
    public string Department { get; set; } = department;
    public string Grade { get; set; } = grade;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public PayBandAuditStatus Status { get; set; } = status;

    /// <summary>Starts a pay band audit in status created.</summary>
    public static PayBandAudit NewPayBandAudit(string code, string label)
    {
        return new PayBandAudit(code, label, "", 0.0, "", "", 0.0, "", "", "", 0, PayBandAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandAuditStatus next)
    {
        switch (Status)
        {
            case PayBandAuditStatus.Created:
                switch (next)
                {
                    case PayBandAuditStatus.Validated:
                        return true;
                }
                return false;
            case PayBandAuditStatus.Validated:
                switch (next)
                {
                    case PayBandAuditStatus.Published:
                    case PayBandAuditStatus.Retired:
                        return true;
                }
                return false;
            case PayBandAuditStatus.Published:
                switch (next)
                {
                    case PayBandAuditStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pay band audit"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Fte + (double)Retries, 2);
    }

    public PayBandAudit Copy()
    {
        return new PayBandAudit(
            Code,
            Label,
            Summary,
            Weight,
            Description,
            ContactEmail,
            Fte,
            Department,
            Grade,
            Title,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
