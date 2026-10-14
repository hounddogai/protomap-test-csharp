namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception quota is in its life cycle.</summary>
public enum ExceptionQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A exception quota as the exception team keeps it.</summary>
public sealed class ExceptionQuota(string code, string label, List<string> tags, string country, int endsAt, ExceptionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public int EndsAt { get; set; } = endsAt;
    public ExceptionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a exception quota in status draft.</summary>
    public static ExceptionQuota NewExceptionQuota(string code, string label)
    {
        return new ExceptionQuota(code, label, new List<string>(), "", 0, ExceptionQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionQuotaStatus next)
    {
        switch (Status)
        {
            case ExceptionQuotaStatus.Draft:
                return next == ExceptionQuotaStatus.Pending || next == ExceptionQuotaStatus.Active;
            case ExceptionQuotaStatus.Pending:
                switch (next)
                {
                    case ExceptionQuotaStatus.Suspended:
                        return true;
                }
                return false;
            case ExceptionQuotaStatus.Active:
                switch (next)
                {
                    case ExceptionQuotaStatus.Suspended:
                    case ExceptionQuotaStatus.Closed:
                        return true;
                }
                return false;
            case ExceptionQuotaStatus.Suspended:
                return next == ExceptionQuotaStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ExceptionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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
        return (double)EndsAt;
    }

    public ExceptionQuota Copy()
    {
        return new ExceptionQuota(Code, Label, Tags, Country, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
