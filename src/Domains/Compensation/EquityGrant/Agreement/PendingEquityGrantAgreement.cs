namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending equity grant agreement is in its life cycle.</summary>
public enum PendingEquityGrantAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a pending equity grant agreement and its current status.</summary>
public sealed class PendingEquityGrantAgreement(string code, string label, string notes, string location, double maximum, string locale, string managerId, Dictionary<string, string> attributes, string name, string employeeId, PendingEquityGrantAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Location { get; set; } = location;
    public double Maximum { get; set; } = maximum;
    public string Locale { get; set; } = locale;
    public string ManagerId { get; set; } = managerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public PendingEquityGrantAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pending equity grant agreement in status open.</summary>
    public static PendingEquityGrantAgreement NewPendingEquityGrantAgreement(string code, string label)
    {
        return new PendingEquityGrantAgreement(
            code,
            label,
            "",
            "",
            0.0,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            PendingEquityGrantAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingEquityGrantAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingEquityGrantAgreementStatus next)
    {
        switch (Status)
        {
            case PendingEquityGrantAgreementStatus.Open:
                switch (next)
                {
                    case PendingEquityGrantAgreementStatus.Archived:
                        return true;
                }
                return false;
            case PendingEquityGrantAgreementStatus.InReview:
                return next == PendingEquityGrantAgreementStatus.Reopened;
            case PendingEquityGrantAgreementStatus.Resolved:
                switch (next)
                {
                    case PendingEquityGrantAgreementStatus.Archived:
                        return true;
                }
                return false;
            case PendingEquityGrantAgreementStatus.Reopened:
                switch (next)
                {
                    case PendingEquityGrantAgreementStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingEquityGrantAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending equity grant agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public PendingEquityGrantAgreement Copy()
    {
        return new PendingEquityGrantAgreement(
            Code,
            Label,
            Notes,
            Location,
            Maximum,
            Locale,
            ManagerId,
            Attributes,
            Name,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
