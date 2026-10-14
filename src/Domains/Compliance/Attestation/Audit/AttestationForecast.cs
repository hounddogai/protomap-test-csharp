namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation forecast is in its life cycle.</summary>
public enum AttestationForecastStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A attestation forecast. Its code is unique within a tenant.</summary>
public sealed class AttestationForecast(string code, string label, bool active, string summary, string jobCode, string name, double amount, AttestationForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string Summary { get; set; } = summary;
    public string JobCode { get; set; } = jobCode;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public AttestationForecastStatus Status { get; set; } = status;

    /// <summary>Starts a attestation forecast in status open.</summary>
    public static AttestationForecast NewAttestationForecast(string code, string label)
    {
        return new AttestationForecast(code, label, false, "", "", "", 0.0, AttestationForecastStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationForecastStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationForecastStatus next)
    {
        switch (Status)
        {
            case AttestationForecastStatus.Open:
                switch (next)
                {
                    case AttestationForecastStatus.Resolved:
                    case AttestationForecastStatus.Reopened:
                        return true;
                }
                return false;
            case AttestationForecastStatus.InReview:
                return next == AttestationForecastStatus.Resolved || next == AttestationForecastStatus.Reopened;
            case AttestationForecastStatus.Resolved:
                return next == AttestationForecastStatus.Reopened;
            case AttestationForecastStatus.Reopened:
                return next == AttestationForecastStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AttestationForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"attestation forecast"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public AttestationForecast Copy()
    {
        return new AttestationForecast(Code, Label, Active, Summary, JobCode, Name, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
