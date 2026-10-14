namespace Acme.Workforce.Contractors.Rate.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate request is in its life cycle.</summary>
public enum RateRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One rate request, with what the planners need to act on it.</summary>
public sealed class RateRequest(string code, string label, string source, string employeeId, string location, double minimum, string phone, Dictionary<string, string> attributes, RateRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string EmployeeId { get; set; } = employeeId;
    public string Location { get; set; } = location;
    public double Minimum { get; set; } = minimum;
    public string Phone { get; set; } = phone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public RateRequestStatus Status { get; set; } = status;

    /// <summary>Starts a rate request in status requested.</summary>
    public static RateRequest NewRateRequest(string code, string label)
    {
        return new RateRequest(
            code,
            label,
            "",
            "",
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            RateRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateRequestStatus next)
    {
        switch (Status)
        {
            case RateRequestStatus.Requested:
                switch (next)
                {
                    case RateRequestStatus.Granted:
                    case RateRequestStatus.Expired:
                        return true;
                }
                return false;
            case RateRequestStatus.Granted:
                switch (next)
                {
                    case RateRequestStatus.Revoked:
                        return true;
                }
                return false;
            case RateRequestStatus.Revoked:
                switch (next)
                {
                    case RateRequestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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
        return Minimum;
    }

    public RateRequest Copy()
    {
        return new RateRequest(Code, Label, Source, EmployeeId, Location, Minimum, Phone, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
