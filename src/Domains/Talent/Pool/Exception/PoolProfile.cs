namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool profile is in its life cycle.</summary>
public enum PoolProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pool profile. Its code is unique within a tenant.</summary>
public sealed class PoolProfile(string code, string label, double maximum, string summary, string source, int createdAt, string notes, string contactEmail, PoolProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public string Source { get; set; } = source;
    public int CreatedAt { get; set; } = createdAt;
    public string Notes { get; set; } = notes;
    public string ContactEmail { get; set; } = contactEmail;
    public PoolProfileStatus Status { get; set; } = status;

    /// <summary>Starts a pool profile in status draft.</summary>
    public static PoolProfile NewPoolProfile(string code, string label)
    {
        return new PoolProfile(code, label, 0.0, "", "", 0, "", "", PoolProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolProfileStatus next)
    {
        switch (Status)
        {
            case PoolProfileStatus.Draft:
                return next == PoolProfileStatus.Pending || next == PoolProfileStatus.Closed;
            case PoolProfileStatus.Pending:
                switch (next)
                {
                    case PoolProfileStatus.Active:
                    case PoolProfileStatus.Suspended:
                        return true;
                }
                return false;
            case PoolProfileStatus.Active:
                switch (next)
                {
                    case PoolProfileStatus.Suspended:
                    case PoolProfileStatus.Closed:
                        return true;
                }
                return false;
            case PoolProfileStatus.Suspended:
                switch (next)
                {
                    case PoolProfileStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pool profile"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Maximum - (double)CreatedAt, 2);
    }

    public PoolProfile Copy()
    {
        return new PoolProfile(Code, Label, Maximum, Summary, Source, CreatedAt, Notes, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
