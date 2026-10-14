namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception request is in its life cycle.</summary>
public enum ExceptionRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A exception request. Its code is unique within a tenant.</summary>
public sealed class ExceptionRequest(string code, string label, bool visible, string location, string externalRef, string ownerId, double rating, string summary, int tenureMonths, ExceptionRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public string OwnerId { get; set; } = ownerId;
    public double Rating { get; set; } = rating;
    public string Summary { get; set; } = summary;
    public int TenureMonths { get; set; } = tenureMonths;
    public ExceptionRequestStatus Status { get; set; } = status;

    /// <summary>Starts a exception request in status created.</summary>
    public static ExceptionRequest NewExceptionRequest(string code, string label)
    {
        return new ExceptionRequest(code, label, false, "", "", "", 0.0, "", 0, ExceptionRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionRequestStatus next)
    {
        switch (Status)
        {
            case ExceptionRequestStatus.Created:
                switch (next)
                {
                    case ExceptionRequestStatus.Validated:
                    case ExceptionRequestStatus.Retired:
                        return true;
                }
                return false;
            case ExceptionRequestStatus.Validated:
                switch (next)
                {
                    case ExceptionRequestStatus.Retired:
                        return true;
                }
                return false;
            case ExceptionRequestStatus.Published:
                switch (next)
                {
                    case ExceptionRequestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)TenureMonths - Rating, 2);
    }

    public ExceptionRequest Copy()
    {
        return new ExceptionRequest(
            Code,
            Label,
            Visible,
            Location,
            ExternalRef,
            OwnerId,
            Rating,
            Summary,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
