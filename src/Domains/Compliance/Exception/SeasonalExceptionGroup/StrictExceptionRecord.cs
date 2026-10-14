namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict exception record is in its life cycle.</summary>
public enum StrictExceptionRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A strict exception record. Its code is unique within a tenant.</summary>
public sealed class StrictExceptionRecord(string code, string title, string badge, double margin, string salaryBand, double hours, int version, string label, int capacity, double ratio, string country, StrictExceptionRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public double Margin { get; set; } = margin;
    public string SalaryBand { get; set; } = salaryBand;
    public double Hours { get; set; } = hours;
    public int Version { get; set; } = version;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public double Ratio { get; set; } = ratio;
    public string Country { get; set; } = country;
    public StrictExceptionRecordStatus Status { get; set; } = status;

    /// <summary>Starts a strict exception record in status requested.</summary>
    public static StrictExceptionRecord NewStrictExceptionRecord(string code, string title)
    {
        return new StrictExceptionRecord(
            code,
            title,
            "",
            0.0,
            "",
            0.0,
            0,
            "",
            0,
            0.0,
            "",
            StrictExceptionRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictExceptionRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictExceptionRecordStatus next)
    {
        switch (Status)
        {
            case StrictExceptionRecordStatus.Requested:
                switch (next)
                {
                    case StrictExceptionRecordStatus.Expired:
                        return true;
                }
                return false;
            case StrictExceptionRecordStatus.Granted:
                return next == StrictExceptionRecordStatus.Revoked;
            case StrictExceptionRecordStatus.Revoked:
                switch (next)
                {
                    case StrictExceptionRecordStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictExceptionRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict exception record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Margin - (double)Version, 2);
    }

    public StrictExceptionRecord Copy()
    {
        return new StrictExceptionRecord(
            Code,
            Title,
            Badge,
            Margin,
            SalaryBand,
            Hours,
            Version,
            Label,
            Capacity,
            Ratio,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
