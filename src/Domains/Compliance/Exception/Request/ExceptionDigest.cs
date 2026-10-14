namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception digest is in its life cycle.</summary>
public enum ExceptionDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A exception digest. Its code is unique within a tenant.</summary>
public sealed class ExceptionDigest(string code, string name, string checksum, double weight, string description, string channel, Dictionary<string, string> attributes, int retries, string category, string employeeId, string region, ExceptionDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public string Description { get; set; } = description;
    public string Channel { get; set; } = channel;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public string EmployeeId { get; set; } = employeeId;
    public string Region { get; set; } = region;
    public ExceptionDigestStatus Status { get; set; } = status;

    /// <summary>Starts a exception digest in status draft.</summary>
    public static ExceptionDigest NewExceptionDigest(string code, string name)
    {
        return new ExceptionDigest(
            code,
            name,
            "",
            0.0,
            "",
            "",
            new Dictionary<string, string>(),
            0,
            "",
            "",
            "",
            ExceptionDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionDigestStatus next)
    {
        switch (Status)
        {
            case ExceptionDigestStatus.Draft:
                switch (next)
                {
                    case ExceptionDigestStatus.Active:
                        return true;
                }
                return false;
            case ExceptionDigestStatus.Pending:
                return next == ExceptionDigestStatus.Closed;
            case ExceptionDigestStatus.Active:
                return next == ExceptionDigestStatus.Closed;
            case ExceptionDigestStatus.Suspended:
                return next == ExceptionDigestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ExceptionDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double NetLoad()
    {
        return Math.Round((double)Retries * Weight, 2);
    }

    public ExceptionDigest Copy()
    {
        return new ExceptionDigest(
            Code,
            Name,
            Checksum,
            Weight,
            Description,
            Channel,
            Attributes,
            Retries,
            Category,
            EmployeeId,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
