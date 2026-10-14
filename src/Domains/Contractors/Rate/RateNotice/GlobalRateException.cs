namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global rate exception is in its life cycle.</summary>
public enum GlobalRateExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A global rate exception. Its code is unique within a tenant.</summary>
public sealed class GlobalRateException(string code, string label, string checksum, string region, string employeeId, List<string> tags, string currency, double score, GlobalRateExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Region { get; set; } = region;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public string Currency { get; set; } = currency;
    public double Score { get; set; } = score;
    public GlobalRateExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a global rate exception in status requested.</summary>
    public static GlobalRateException NewGlobalRateException(string code, string label)
    {
        return new GlobalRateException(
            code,
            label,
            "",
            "",
            "",
            new List<string>(),
            "",
            0.0,
            GlobalRateExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalRateExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalRateExceptionStatus next)
    {
        switch (Status)
        {
            case GlobalRateExceptionStatus.Requested:
                switch (next)
                {
                    case GlobalRateExceptionStatus.Granted:
                    case GlobalRateExceptionStatus.Expired:
                        return true;
                }
                return false;
            case GlobalRateExceptionStatus.Granted:
                return next == GlobalRateExceptionStatus.Revoked;
            case GlobalRateExceptionStatus.Revoked:
                switch (next)
                {
                    case GlobalRateExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalRateExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global rate exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"global rate exception"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public GlobalRateException Copy()
    {
        return new GlobalRateException(Code, Label, Checksum, Region, EmployeeId, Tags, Currency, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
